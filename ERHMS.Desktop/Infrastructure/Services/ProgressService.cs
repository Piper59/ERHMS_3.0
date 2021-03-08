﻿using ERHMS.Desktop.Services;
using ERHMS.Desktop.ViewModels;
using ERHMS.Desktop.Views;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ERHMS.Desktop.Infrastructure.Services
{
    public class ProgressService : IProgressService
    {
        private static readonly TimeSpan ShowDialogDelay = TimeSpan.FromSeconds(1.0);

        private ProgressViewModel viewModel;

        public Application Application { get; }

        public ProgressService(Application application)
        {
            Application = application;
        }

        public void Report(string value)
        {
            Application.Dispatcher.Invoke(() => viewModel.Status = value);
        }

        private async Task RunCoreAsync(string title, bool canBeCanceled, Action<CancellationToken> action)
        {
            viewModel = new ProgressViewModel(title, canBeCanceled);
            try
            {
                Window owner = Application.GetActiveOrMainWindow();
                Window dialog = new ProgressView
                {
                    Owner = owner,
                    DataContext = viewModel
                };
                using (owner.BeginDisable())
                using (CancellationTokenSource completionTokenSource = new CancellationTokenSource())
                {
                    Task task = Task.Run(() =>
                    {
                        action(viewModel.CancellationToken);
                        completionTokenSource.Cancel();
                    });
                    try
                    {
                        await Task.Delay(ShowDialogDelay, completionTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        await task;
                        return;
                    }
                    using (dialog.BeginShowDialog())
                    {
                        await task;
                    }
                }
            }
            finally
            {
                viewModel.Dispose();
                viewModel = null;
            }
        }

        public async Task RunAsync(string title, Action action)
        {
            await RunCoreAsync(title, false, _ => action());
        }

        public async Task RunAsync(string title, Action<CancellationToken> action)
        {
            await RunCoreAsync(title, true, action);
        }
    }
}
