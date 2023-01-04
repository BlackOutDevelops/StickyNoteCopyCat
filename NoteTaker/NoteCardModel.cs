using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker
{
    public class NoteCardModel
    {
        public int Id { get; set; }
        
        public string Note { get; set; }

        public string ImagePaths { get; set; }

        public string UpdatedTime { get; set; }
    }
}
