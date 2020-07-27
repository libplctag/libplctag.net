﻿namespace libplctag.DataTypes
{
    public class DintMarshaller : IMarshaller<int>
    {
        public int ElementSize => 4;
        public PlcType PlcType { get; set; }
        public int Decode(Tag tag, int offset) => tag.GetInt32(offset);
        public void Encode(Tag tag, int offset, int value) => tag.SetInt32(offset, value);
    }
}