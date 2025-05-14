using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlazorDatastar.Utils
{
    public class WorkDispatcher<T>
    {
        private readonly ConcurrentDictionary<Guid, Channel<T>> _channels = new();

        public Writer CreateWriter() => new Writer(_channels);

        public Reader CreateReader()
        {
            var channel = Channel.CreateUnbounded<T>();
            var id = Guid.NewGuid();
            _channels[id] = channel;
            return new Reader(id, channel.Reader, _channels);
        }

        public class Writer : IDisposable
        {
            private readonly ConcurrentDictionary<Guid, Channel<T>> _channels;

            public Writer(ConcurrentDictionary<Guid, Channel<T>> channels)
            {
                _channels = channels;
            }

            public async Task WriteAsync(T item, CancellationToken cancellationToken = default)
            {
                foreach (var channel in _channels.Values)
                {
                    await channel.Writer.WriteAsync(item, cancellationToken);
                }
            }

            public void Dispose()
            {
            }
        }

        public class Reader : IAsyncDisposable
        {
            private readonly Guid _id;
            private readonly ChannelReader<T> _reader;
            private readonly ConcurrentDictionary<Guid, Channel<T>> _channels;

            public Reader(Guid id, ChannelReader<T> reader, ConcurrentDictionary<Guid, Channel<T>> channels)
            {
                _id = id;
                _reader = reader;
                _channels = channels;
            }

            public async IAsyncEnumerable<T> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                while (await _reader.WaitToReadAsync(cancellationToken))
                {
                    while (_reader.TryRead(out var item))
                    {
                        yield return item;
                    }
                }
            }

            public ValueTask DisposeAsync()
            {
                if (_channels.TryRemove(_id, out var channel))
                {
                    channel.Writer.Complete();
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
