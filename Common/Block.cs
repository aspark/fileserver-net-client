using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client.Common
{
    abstract class BlockBase { }

    internal interface IStringBlock { }

    class ErrorBlock : BlockBase, IStringBlock
    {
        public ErrorBlock(string err)
        {
            Error = err;
        }

        public string Error { get; private set; }

        public override string ToString()
        {
            return "ERR: " + Error;
        }
    }

    class StringBlock : BlockBase, IStringBlock
    {
        public StringBlock(string str)
        {
            Value = str;
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }
    }

    class BytesBlock : BlockBase
    {
        public BytesBlock(byte[] val)
        {
            Value = val;
        }

        public byte[] Value { get; private set; }
    }

    class Int64Block : BlockBase
    {
        public Int64Block(long val)
        {
            Value = val;
        }

        public long Value { get; private set; }
    }

    class ArrayBlock : BlockBase
    {
        public ArrayBlock(BlockBase[] items)
        {
            Blocks = items;
        }

        public BlockBase[] Blocks { get; private set; }
    }
}
