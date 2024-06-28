
using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class MathNETutils
{
    public static Vector<double> AddElementsToVector(Vector<double> originalVec, int nNewElements, double newValue)
    {
        int originalSize = originalVec.Count;
        int newSize = originalSize + nNewElements;
        Vector<double> newVec = Vector<double>.Build.Dense(newSize);

        for (int i=0; i<originalSize; i++)
        {
            newVec[i] = originalVec[i];
        }
        for (int i=originalSize; i<newSize; i++)
        {
            newVec[i] = newValue;
        }
        return newVec;
    }

    public static Matrix<double> AddRowsAndColsToSquareMatrix(Matrix<double> originalMatrix, int nNewRowAndCols, double diagValue, double offDiagValue)
    {
        int originalSize = originalMatrix.RowCount;
        int newSize = originalSize + nNewRowAndCols;
        Matrix<double> newMatrix = Matrix<double>.Build.Dense(newSize, newSize);

        for (int i=0; i<originalSize; i++)
        {
            for (int j=0; j<originalSize; j++)
            {
                newMatrix[i, j] = originalMatrix[i, j];
            }
        }
        for (int i=originalSize; i<newSize; i++)
        {
            for (int j=originalSize; j<newSize; j++)
            {
                if (i != j)
                {
                    newMatrix[i, j] = offDiagValue;
                }
                else
                {
                    newMatrix[i, j] = diagValue;
                }
            }
        }
        return newMatrix;
    }
    public static Vector<double> RemoveElementsFromVector(Vector<double> originalVec, List<int> indicesToRemove)
    {
        int originalSize = originalVec.Count;
        int nElemToRemove = indicesToRemove.Count;
        int newSize = originalVec.Count - nElemToRemove;
        Vector<double> newVec = Vector<double>.Build.Dense(newSize);
        int newVecIndex = 0;
        for (int i=0; i<originalSize; i++)
        {
            bool removeCurrentElem = indicesToRemove.Any(index => index == i);
            if (!removeCurrentElem)
            {
                newVec[newVecIndex] = originalVec[i];
                newVecIndex += 1;
            }
        }
        return newVec;
    }
    public static Matrix<double> RemoveRowsAndColsFromSquareMatrix(Matrix<double> originalMatrix, List<int> indicesToRemove)
    {
        int originalSize = originalMatrix.RowCount;
        int nElemToRemove = indicesToRemove.Count;
        int newSize = originalMatrix.RowCount - nElemToRemove;
        Matrix<double> newMatrix = Matrix<double>.Build.Dense(newSize, newSize);
        int newMatrixRowIndex = 0;
        for (int i=0; i<originalSize; i++)
        {
            bool removeCurrentRow = indicesToRemove.Any(index => index == i);
            if (!removeCurrentRow)
            {
                int newMatrixColIndex = 0;
                for (int j=0; j<originalSize; j++)
                {
                    bool removeCurrentElem = indicesToRemove.Any(index => index == j);
                    if (!removeCurrentElem)
                    {
                        newMatrix[newMatrixRowIndex, newMatrixColIndex] = originalMatrix[i, j];
                        newMatrixColIndex += 1;
                    }
                }
                newMatrixRowIndex += 1;
            }
        }
        return newMatrix;
    }
}
