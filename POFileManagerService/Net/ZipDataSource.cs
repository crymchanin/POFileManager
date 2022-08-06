using ICSharpCode.SharpZipLib.Zip;
using System.IO;


namespace POFileManagerService.Net {
    public class ZipDataSource : IStaticDataSource {

        private byte[] _data;

        public ZipDataSource(byte[] data) {
            _data = data;
        }

        public Stream GetSource() {
            return new MemoryStream(_data);
        }
    }
}
