// See https://aka.ms/new-console-template for more information

// Online Bayesian linear regression
//
// The code below uses an online algorithm to estimate the posterior of the weighs
// of a linear regression model using simulate data.


import joacorapela_common.stats.bayesianLinearRegression

// Define data generation variables

int n_samples = 20;
int n_post_samples = 6;
double prior_precision_coef = 2.0;
int[] n_samples_to_plot = {1, 2, 20};
string data_filename_pattern = "data/linearRegression_nSamples{:02d}.npz";
string fig_filename_pattern = "figures/onlineBayesianLinearRegression_nSamples{:02d}.{:s}";

// Load data

string data_filename = String.Format(data_filename_pattern, n_samples);
var load_res = MatlabReader.ReadAll<double>(data_filename, "x", "t", "a0", "a1", "sigma");
double[] independent_var = load_res["x"]
double[] dependent_var = load_res["t"]
doua0 = load_res["a0"]
a1 = load_res["a1"]
sigma = load_res["sigma"]
likelihood_precision_coef = (1.0/sigma)**2

// Estimate and plot posterior

marker_true = "cross"
size_true = 10
color_true = "white"
marker_data = "circle-open"
size_data = 10
color_data = "blue"
line_width_data = 5
x = np.linspace(-1, 1, 100)
y = np.linspace(-1, 1, 100)
X, Y = np.meshgrid(x, y)
pos = np.dstack((X, Y))

Phi = np.column_stack((np.ones(len(independent_var)), independent_var))
alpha = prior_precision_coef
beta = likelihood_precision_coef

# set prior
m0 = np.array([0.0, 0.0])
S0 = 1.0 / alpha * np.eye(2)
#

fig = plotly.subplots.make_subplots(rows=len(n_samples_to_plot)+1, cols=3)
x_m1_1 = np.arange(-1.0, 1.0, 0.1)

# trace true coefficient
trace_true_coef = go.Scatter(x=[a0], y=[a1], mode="markers",
                             marker_symbol=marker_true,
                             marker_size=size_true,
                             marker_color=color_true,
                             name="true mean",
                             showlegend=False)

rv = scipy.stats.multivariate_normal(m0, S0)

# plot prior
Z = rv.pdf(pos)
trace_post = go.Contour(x=x, y=y, z=Z, showscale=False)
fig.add_trace(trace_post, row=1, col=2)

fig.add_trace(trace_true_coef, row=1, col=2)

fig.update_xaxes(title_text="Intercept", row=1, col=2)
fig.update_yaxes(title_text="Slope", row=1, col=2)

# sample from prior
samples = rv.rvs(size=n_post_samples)

# plot regression lines corresponding to samples
for a_sample in samples:
    sample_intercept, sample_slope = a_sample
    sample_y = sample_intercept + sample_slope * x_m1_1
    trace = go.Scatter(x=x_m1_1, y=sample_y, mode="lines",
                       line_color="red", showlegend=False)
    fig.add_trace(trace, row=1, col=3)
fig.update_xaxes(title_text="x", row=1, col=3)
fig.update_yaxes(title_text="y", row=1, col=3)

mn = m0
Sn = S0
for n, t in enumerate(dependent_var):
    print(f"Processing {n}/({len(dependent_var)})")
    # update posterior
    mn, Sn = joacorapela_common.stats.bayesianLinearRegression.onlineUpdate(
        mn=mn, Sn=Sn, phi=Phi[n, :], y=t, alpha=alpha, beta=beta)

    if n+1 in n_samples_to_plot:
        index_sample = n_samples_to_plot.index(n+1)
        # compute likelihood
        Z = np.empty(shape=(len(x), len(y)), dtype=np.double)
        for i, w0 in enumerate(x):
            for j, w1 in enumerate(y):
                rv = scipy.stats.norm(w0 + w1 * independent_var[n], sigma)
                Z[j, i] = rv.pdf(t)

        # plot likelihood
        trace_like = go.Contour(x=x, y=y, z=Z, showscale=False)
        fig.add_trace(trace_like, row=index_sample+2, col=1)

        fig.add_trace(trace_true_coef, row=index_sample+2, col=1)

        fig.update_xaxes(title_text="Intercept", row=index_sample+2, col=1)
        fig.update_yaxes(title_text="Slope", row=index_sample+2, col=1)

        rv = scipy.stats.multivariate_normal(mn, Sn)

        # plot updated posterior
        Z = rv.pdf(pos)
        trace_post = go.Contour(x=x, y=y, z=Z, showscale=False)
        fig.add_trace(trace_post, row=index_sample+2, col=2)

        fig.add_trace(trace_true_coef, row=index_sample+2, col=2)

        fig.update_xaxes(title_text="Intercept", row=index_sample+2, col=2)
        fig.update_yaxes(title_text="Slope", row=index_sample+2, col=2)

        # sample from posterior
        samples = rv.rvs(size=n_post_samples)

        # plot regression lines corresponding to samples
        for a_sample in samples:
            sample_intercept, sample_slope = a_sample
            sample_y = sample_intercept + sample_slope * x_m1_1
            trace = go.Scatter(x=x_m1_1, y=sample_y, mode="lines",
                               line_color="red", showlegend=False)
            fig.add_trace(trace, row=index_sample+2, col=3)
        trace_data = go.Scatter(x=independent_var[:(n+1)], y=dependent_var[:(n+1)],
                                mode="markers",
                                marker_symbol=marker_data,
                                marker_size=size_data,
                                marker_color=color_data,
                                marker_line_width=line_width_data,
                                showlegend=False,
                               )
        fig.add_trace(trace_data, row=index_sample+2, col=3)
        fig.update_xaxes(title_text="x", row=index_sample+2, col=3)
        fig.update_yaxes(title_text="y", row=index_sample+2, col=3)

png_fig_filename = fig_filename_pattern.format(n_samples, "png")
html_fig_filename = fig_filename_pattern.format(n_samples, "html")
fig.write_image(png_fig_filename)
fig.write_html(html_fig_filename)

fig
