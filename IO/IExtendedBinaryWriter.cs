﻿namespace IO
{
	public interface IExtendedBinaryWriter
	{
		void Write(bool b);
		void Write(byte b);
        void Write(byte[] buffer);
        void Write(ushort value);
        void Write(uint value);
	    void WriteOpt(ushort value);
		void WriteOpt(int value);
		void WriteOpt(long value);
		void WriteOptAscii(string s);
	}
}