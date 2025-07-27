using System.IO;

namespace QuazalWV
{
    public interface IData
    {
        void FromStream(Stream s);
        void ToBuffer(Stream s);
    }
}
