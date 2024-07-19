
using System;
using System.IO;
using System.Collections.Generic;
using System.Reactive.Linq;

public class FileSystemObserver {
    static public IObservable<FileSystemEventArgs> CreateFileSystemObservable(string folder)
    {
        return
            // Observable.Defer enables us to avoid doing any work
            // until we have a subscriber.
            Observable.Defer(() =>
                {
                    FileSystemWatcher fsw = new(folder);
                    fsw.EnableRaisingEvents = true;

                    return Observable.Return(fsw);
                })
            // Once the preceding part emits the FileSystemWatcher
            // (which will happen when someone first subscribes), we
            // want to wrap all the events as IObservable<T>s, for which
            // we'll use a projection. To avoid ending up with an
            // IObservable<IObservable<FileSystemEventArgs>>, we use
            // SelectMany, which effectively flattens it by one level.
            .SelectMany(fsw =>
                Observable.Merge(new[]
                    {
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Created += h, h => fsw.Created -= h),
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Changed += h, h => fsw.Changed -= h),
                        Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                            h => fsw.Renamed += h, h => fsw.Renamed -= h),
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Deleted += h, h => fsw.Deleted -= h)
                    })
                // FromEventPattern supplies both the sender and the event
                // args. Extract just the latter.
                .Select(ep => ep.EventArgs)
                // The Finally here ensures the watcher gets shut down once
                // we have no subscribers.
                .Finally(() => fsw.Dispose()))
            // This combination of Publish and RefCount means that multiple
            // subscribers will get to share a single FileSystemWatcher,
            // but that it gets shut down if all subscribers unsubscribe.
            .Publish()
            .RefCount();
    }
}
