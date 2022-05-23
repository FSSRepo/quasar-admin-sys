using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class ByteStream
{

    byte[] data;
    public int offset = 0;

    public ByteStream(int lenght)
    {
        data = new byte[lenght];
    }
    public void writeFloatArray(float[] data)
    {
        for (int ofs = 0; ofs < data.Length; ofs++)
        {
            writeFloat(data[ofs]);
        }
    }

    public void writeByteArray(byte[] buffer)
    {

        for (int i = 0; i < buffer.Length; i++)
        {
            this.data[offset] = buffer[i];
            offset++;
        }
    }

    public void writeShortArray(short[] data)
    {
        for (int ofs = 0; ofs < data.Length; ofs++)
        {
            writeShort(data[ofs]);
        }
    }

    public void writeFloat2(float x, float y)
    {
        writeFloat(x);
        writeFloat(y);
    }

    public void writeFloat3(float x, float y, float z)
    {
        writeFloat(x);
        writeFloat(y);
        writeFloat(z);
    }



    public void writeString(String text)
    {
        writeShort(text.Length);
        writeByteArray(Encoding.Default.GetBytes(text));
    }

    public void writeStringFromSize(int size, String text)
    {
        byte[] data = new byte[size];
        for (short i = 0; i < size; i++)
        {
            if (i < text.Length)
            {
                data[i] = (byte)text[i];
            }
            else
            {
                break;
            }
        }
        writeByteArray(data);
    }

    public void writeFloat(float val)
    {
        writeInt(BitConverter.ToInt32(BitConverter.GetBytes(val), 0));
    }

    public void writeInt(int values)
    {

        data[offset] = (byte)(values & 0xFF);
        data[offset + 1] = (byte)((values >> 8) & 0xFF);
        data[offset + 2] = (byte)((values >> 16) & 0xFF);
        data[offset + 3] = (byte)((values >> 24) & 0xFF);
        offset += 4;
    }

    public void writeShort(int values)
    {

        data[offset] = (byte)(values & 0xFF);
        data[offset + 1] = (byte)((values >> 8) & 0xFF);
        offset += 2;
    }

    public void writeByte(int values)
    {
        data[offset] = (byte)(values & 0xFF);
        offset++;
    }

    public int readUShort()
    {
        return ((read() & 0xFF) | (read() & 0xFF) << 8);
    }
    public short readShort()
    {
        return (short)(read() | (read() << 8));
    }

    public short readUbyte()
    {
        return (short)(read() & 0xFF);
    }

    public byte readByte()
    {
        return (byte)read();
    }

    public Boolean readBoolean()
    {
        return read() == 1;
    }
    public int read()
    {
        return data[offset++];

    }

    public byte[] getBuffer()
    {
        return data;
    }
    public int readInt()
    {
        return (read() & 0xFF)
            | (read() & 0xFF) << 8
            | (read() & 0xFF) << 16
            | (read() & 0xFF) << 24;
    }


    public short[] readShortArray(int size)
    {
        short[] dat = new short[size];
        byte[] buffer = readByteArray(size * 2);
        for (int i = 0; i < size; i++)
        {
            dat[i] = (short)((buffer[i * 2] & 0xFF) | (buffer[i * 2 + 1] & 0xFF) << 8);
        }
        return dat;
    }

    public void skip(int len)
    {
        offset += len;
    }


    public float readFloat()
    {
        return BitConverter.ToSingle(BitConverter.GetBytes(readInt()), 0);
    }

    public byte[] readByteArray(int size)
    {
        byte[] buffer = new byte[size];
        for (int i = 0; i < size; i++)
        {
            buffer[i] = (byte)(data[offset + i]);

        }
        offset += size;
        return buffer;
    }

    public string readString()
    {
        short lenght = readShort();
        return Encoding.Default.GetString(readByteArray(lenght));
    }

    public void rewind()
    {
        offset = 0;
    }
}

