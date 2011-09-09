﻿#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Lokad.Cqrs.Feature.TapeStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense
    {
        IAtomicStorageFactory _atomicStorageFactory;
        IStreamingRoot _streamingRoot;
        public readonly ISystemObserver Observer;
        ITapeStorageFactory _tapeStorage;


        public void AtomicIs(IAtomicStorageFactory factory)
        {
            _atomicStorageFactory = factory;
        }

        //public void AtomicIs(Func<IComponentContext>)
        public void AtomicIsInMemory()
        {
            AtomicIsInMemory(builder => { });
        }

        public void AtomicIsInMemory(Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var dictionary = new ConcurrentDictionary<string, byte[]>();
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new MemoryAtomicStorageFactory(dictionary, builder.Build()));
        }

        public void AtomicIsInFiles(string folder, Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new FileAtomicStorageFactory(folder, builder.Build()));
        }

        public void TapeIs(ITapeStorageFactory storage)
        {
            _tapeStorage = storage;
        }

        public void TapeIsInMemory()
        {
            var storage = new ConcurrentDictionary<string, List<byte[]>>();
            var factory = new MemoryTapeStorageFactory(storage);
            TapeIs(factory);
        }

        public void TapeIsInFiles(string fullPath)
        {
            var factory = new FileTapeStorageFactory(fullPath);
            TapeIs(factory);
        }

        public void AtomicIsInFiles(string folder)
        {
            AtomicIsInFiles(folder, builder => { });
        }


        

        public StorageModule(ISystemObserver observer)
        {
            Observer = observer;
        }


        public void StreamingIsInFiles(string filePath)
        {
            _streamingRoot = new FileStreamingContainer(filePath);
        }

        public void StreamingIs(IStreamingRoot streamingRoot)
        {
            _streamingRoot = streamingRoot;
        }

        public void Configure(Container container)
        {
            if (_atomicStorageFactory == null)
            {
                AtomicIsInMemory(strategyBuilder => { });
            }
            if (_streamingRoot == null)
            {
                StreamingIsInFiles(Directory.GetCurrentDirectory());
            }
            if (_tapeStorage == null)
            {
                TapeIsInMemory();
            }

            container.Sources.Push(new AtomicRegistrationCore());
            container.Register(new NuclearStorage(_atomicStorageFactory));
            container.Register(_atomicStorageFactory);

            var setup = container.TryResolve<EngineSetup>();
            if (null != setup)
            {
                var process = new AtomicStorageInitialization(new[] {_atomicStorageFactory},
                    container.Resolve<ISystemObserver>());
                setup.AddProcess(process);
            }


            container.Register(_streamingRoot);

            container.Register(_tapeStorage);
            container.Register<IEngineProcess>(new TapeStorageInitilization(new[] {_tapeStorage}));
        }
    }
}