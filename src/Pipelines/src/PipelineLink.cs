using System;
using System.Threading.Tasks;

namespace Pipelines
{
    internal class PipelineLink
    {
        internal PipelineLink PrevLink { get; set; }
        internal PipelineLink NextLink { get; set; }
        internal Func<object, object> Func { get; set; }
    }

    internal class PipelineLink<TIn, TFirst> : PipelineLink, IPipelineLink<TIn, TFirst>
    {
        internal PipelineLink(PipelineLink prev = null)
        {
            PrevLink = prev;

            if (prev != null)
            {
                prev.NextLink = this;
            }
        }

        private PipelineLink GetFirstPipelineLink()
        {
            var link = PrevLink;

            do
            {
                link = link.PrevLink;
            }
            while (link.PrevLink != null);

            return link;
        }

        public Func<TFirst, Task<TIn>> EndAsync()
        {
            var link = GetFirstPipelineLink();

            return input => Task.FromResult((TIn)link.Func.Invoke(input));
        }

        public Func<TFirst, TIn> End()
        {
            var link = GetFirstPipelineLink();

            return input => (TIn)link.Func.Invoke(input);
        }

        public IPipelineLink<TOut, TFirst> Next<TOut>(Func<TIn, TOut> func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = input =>
            {
                var output = func.Invoke((TIn)input);

                if (NextLink?.Func is null)
                {
                    return output;
                }

                return NextLink.Func.Invoke(output);
            };

            return new PipelineLink<TOut, TFirst>(this);
        }
    }
}