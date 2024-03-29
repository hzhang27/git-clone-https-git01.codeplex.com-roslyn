﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities
{
    internal static partial class TaskFactoryExtensions
    {
        public static Task SafeStartNew(this TaskFactory factory, Action action, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            return factory.SafeStartNew(action, cancellationToken, TaskCreationOptions.None, scheduler);
        }

        public static Task SafeStartNew(
            this TaskFactory factory,
            Action action,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            Action wrapped = () =>
            {
                try
                {
                    action();
                }
                catch (Exception e) if (FatalError.ReportUnlessCanceled(e))
                {
                    throw ExceptionUtilities.Unreachable;
                }
            };

            // The one and only place we can call StartNew().
            return factory.StartNew(wrapped, cancellationToken, creationOptions, scheduler);
        }

        public static Task<TResult> SafeStartNew<TResult>(this TaskFactory factory, Func<TResult> func, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            return factory.SafeStartNew(func, cancellationToken, TaskCreationOptions.None, scheduler);
        }

        public static Task<TResult> SafeStartNew<TResult>(
            this TaskFactory factory,
            Func<TResult> func,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            Func<TResult> wrapped = () =>
            {
                try
                {
                    return func();
                }
                catch (Exception e) if (FatalError.ReportUnlessCanceled(e))
                {
                    throw ExceptionUtilities.Unreachable;
                }
            };

            // The one and only place we can call StartNew<>().
            return factory.StartNew(wrapped, cancellationToken, creationOptions, scheduler);
        }

        public static Task SafeStartNewFromAsync(this TaskFactory factory, Func<Task> actionAsync, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            return factory.SafeStartNewFromAsync(actionAsync, cancellationToken, TaskCreationOptions.None, scheduler);
        }

        public static Task SafeStartNewFromAsync(
            this TaskFactory factory,
            Func<Task> actionAsync,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            // The one and only place we can call StartNew<>().
            var task = factory.StartNew(actionAsync, cancellationToken, creationOptions, scheduler).Unwrap();

            // make it crash if exception has thrown
            task.ContinueWith(t => FatalError.Report(t.Exception),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return task;
        }

        public static Task<TResult> SafeStartNewFromAsync<TResult>(this TaskFactory factory, Func<Task<TResult>> funcAsync, CancellationToken cancellationToken, TaskScheduler scheduler)
        {
            return factory.SafeStartNewFromAsync(funcAsync, cancellationToken, TaskCreationOptions.None, scheduler);
        }

        public static Task<TResult> SafeStartNewFromAsync<TResult>(
            this TaskFactory factory,
            Func<Task<TResult>> funcAsync,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            // The one and only place we can call StartNew<>().
            var task = factory.StartNew(funcAsync, cancellationToken, creationOptions, scheduler).Unwrap();

            // make it crash if exception has thrown
            task.ContinueWith(t => FatalError.Report(t.Exception),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return task;
        }
    }
}
