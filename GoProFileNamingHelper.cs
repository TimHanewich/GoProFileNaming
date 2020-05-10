using System;
using System.Collections.Generic;

namespace GoProFileNaming
{
    public class GoProFileNamingHelper
    {
        public SuggestedFileNameChange[] GetNewFileNames(string[] file_names)
        {
            //Parse the names
            List<ParsedGoProFileName> gpfns = new List<ParsedGoProFileName>();
            foreach (string s in file_names)
            {
                try
                {
                    ParsedGoProFileName gpfn = new ParsedGoProFileName(s);
                    gpfns.Add(gpfn);
                }
                catch
                {
                    throw new Exception("Critical failure: unable to parse file name '" + s + "'.");
                }
            }
        
            //Arrange by Video ID, lowest to highest (Filter1)
            List<ParsedGoProFileName> Filter1 = new List<ParsedGoProFileName>();
            do
            {
                ParsedGoProFileName winner = gpfns[0];
                foreach (ParsedGoProFileName pgfn in gpfns)
                {
                    int this_vid_num = Convert.ToInt32(pgfn.Number);
                    int winner_vid_num = Convert.ToInt32(winner.Number);
                    if (this_vid_num < winner_vid_num)
                    {
                        winner = pgfn;
                    }
                }
                Filter1.Add(winner);
                gpfns.Remove(winner);
            } while (gpfns.Count > 0);

            //Arrange by Chapter number (lowest to highest) (Filter2)
            //Get all unique video ID's
            List<string> UniqueVideoIds = new List<string>();
            foreach (ParsedGoProFileName pgpfn in Filter1)
            {
                if (UniqueVideoIds.Contains(pgpfn.Number) == false)
                {
                    UniqueVideoIds.Add(pgpfn.Number);
                }
            }
            //Now that we have all unique video ID's, arrange them by Chapter number.
            List<ParsedGoProFileName> Filter2 = new List<ParsedGoProFileName>();
            foreach (string s in UniqueVideoIds)
            {
                //Get all videos with this ID
                List<ParsedGoProFileName> FilesForThisVideo = new List<ParsedGoProFileName>();
                foreach (ParsedGoProFileName pgpfn in Filter1)
                {
                    if (pgpfn.Number == s)
                    {
                        FilesForThisVideo.Add(pgpfn);
                    }
                }

                //Now add them by Chapter from lowest to highest to the main Filter2
                do
                {
                    ParsedGoProFileName winner = FilesForThisVideo[0];
                    foreach (ParsedGoProFileName pgpfn in FilesForThisVideo)
                    {
                        int this_chapter = Convert.ToInt32(pgpfn.Chapter);
                        int winner_chapter = Convert.ToInt32(pgpfn.Chapter);
                        if (this_chapter < winner_chapter)
                        {
                            winner = pgpfn;
                        }
                    }
                    Filter2.Add(winner);
                    FilesForThisVideo.Remove(winner);
                } while (FilesForThisVideo.Count > 0);
            }
        
            //Create the file name changes and return those.
            List<SuggestedFileNameChange> name_changes = new List<SuggestedFileNameChange>();
            int t = 0;
            for (t=0;t<Filter2.Count;t++)
            {
                SuggestedFileNameChange sfnc = new SuggestedFileNameChange();
                sfnc.OldName = Filter2[t].ToString();
                sfnc.NewName = (t+1).ToString() + Filter2[t].Extension;
                name_changes.Add(sfnc);
            }
            return name_changes.ToArray();
            
        }

        private class ParsedGoProFileName
        {
            public string Prefix {get; set;}
            public string Chapter {get; set;}
            public string Number {get; set;}
            public string Extension {get; set;}

            public ParsedGoProFileName(string file_name)
            {
                if (file_name.Length != 12)
                {
                    throw new Exception("Unable to parse GoPro file name.  Please be sure you are passing the full file name, including the file extension.  Example: 'GH010670.MP4'");
                }
                
                Prefix = file_name.Substring(0, 2);
                Chapter = file_name.Substring(2, 2);
                Number = file_name.Substring(4, 4);
                Extension = file_name.Substring(9, 3);

                //Check for number compatibility
                try
                {
                    int i = Convert.ToInt32(Chapter);
                }
                catch
                {
                    throw new Exception("Chapter '" + Chapter + "' is not a valid number.");
                }
                try
                {
                    int i = Convert.ToInt32(Number);
                }
                catch
                {
                    throw new Exception("File number '" + Number + "' is not a valid number.");
                }
            }

            public override string ToString()
            {
                return Prefix + Chapter + Number + "." + Extension;
            }
        }
    }
}