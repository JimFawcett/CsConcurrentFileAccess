///////////////////////////////////////////////////////////////////////
// FileReader.cs - Handle concurrent read accesses to the same file  //
//                                                                   //
// Language:    C#, ver 1.2                                          //
// Platform:    Dell Dimension 8300, Windows XP Pro, SP1             //
// Application: Prototype for Project #5, CSE681, Summer 2004        //
// Author:      Jim Fawcett, Syracuse University                     //
//              jfawcett@twcny.rr.com, (315) 443-3948                //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * This module supports safe concurrent file access for reading while
 * the file may also be open for writing.
 *
 * Public Interface
 * ================
 * FileReader fr = new FileReader();
 * bool ok = fr.Open(fileSpec);
 * fr.Close();
 * int NumBytesRead = fr.ReadFile();
 * byte[] bytes = fr.LastFileRead();
 * int NumAttempts = fr.NumAttempts;
 * int NumFailures = fr.NumFailures;
 * fr.SleepMilliSecs = 100;
 */
/*
 * Build Process
 * =============
 * csc /d:TEST_FILEREADER FileReader.cs
 * 
 * Maintenance History
 * =================== 
 * ver 1.2 : 17 Nov 04
 * - Cosmetic additions and changes to these comments.
 * ver 1.1 : 22 Oct 03
 * - fixed a problem with closing, identified by Carmen Vaca Ruiz.
 *   Thanks Carmen.
 * ver 1.0 : 12 Oct 03
 * - first release
 */

//
using System;
using System.IO;
using System.Threading;

namespace FileLocks
{
  //----< open and read file for multiple readers >--------------------

  class FileReader
  {
    FileStream fs = null;
    byte[] _block = null;
    int _NumAttempts = 50;
    int _NumFails = 0;
    int _SleepMilliSecs = 50;

    //----< attempt to open file for shared reading >------------------

    public bool Open(string infile)
    {
      bool retVal = false;
      for(int j=0; j<_NumAttempts; ++j)
      {
        try
        {
          fs = File.Open(
            infile,FileMode.Open,FileAccess.Read,FileShare.Read
          );
          retVal = true;
          break;
        }
        catch(Exception)
        {
          Thread.Sleep(_SleepMilliSecs);
          retVal = false;
        }
      }
      if(retVal==false)
        ++_NumFails;
      return retVal;
    }
    //----< close file >-----------------------------------------------

    public void Close()
    {
      try
      {
        fs.Close();
      }
      catch
      {
        // stream not initialized
      }
    }
    //
    //----< read file into an internal byte array >--------------------

    public int ReadFile(string file)
    {
      FileInfo fi = new FileInfo(file);
      _block = new Byte[fi.Length];
      int NumBytesRead = 0;
      if(Open(file))
      {
        NumBytesRead = fs.Read(_block,0,(int)fi.Length);
        Close();
      }
      return NumBytesRead;
    }
    //----< access byte array of last file read >----------------------

    public byte[] LastFileRead()
    {
      return _block;
    }
    //----< get/set number of opens to attempt >-----------------------

    public int NumAttempts
    {
      get { return _NumAttempts;  }
      set { _NumAttempts = value; }
    }
    //----< report the number of failures to open >--------------------

    public int NumFailures
    {
      get { return _NumFails; }
    }
    //----< get/set number of msecs to sleep after Open failure >------

    public int SleepMilliSecs
    {
      get { return _SleepMilliSecs;  }
      set { _SleepMilliSecs = value; }
    }
  }
  //
  //----< test stub >--------------------------------------------------

#if(TEST_FILEREADER)

  class TestFileReader
  {
    public string Title(string s)
    {
      int len = s.Length;
      string underline = new string('=',len+2);
      string temp = "\n  " + s + "\n" + underline;
      return temp;
    }

    [STAThread]
    static void Main(string[] args)
    {
      FileReader fr = new FileReader();
      TestFileReader tfr = new TestFileReader();
      Console.Write(tfr.Title("Test File Locking"));
      
      if(args.Length == 0)
      {
        Console.Write("\n  Please enter name of file to read");
        return;
      }

      string path = Path.GetFullPath("../../..");
      path += "\\" + args[0];

      const int NumCycles = 25000;
      int size = 0;
      for(int i=0; i<NumCycles; ++i)
      {
        size = fr.ReadFile(path);
        if(size == 0)
          continue;
        Console.Write("\n  {0,5}: read {1} bytes from {2}",i,size,args[0]);
      }
      Console.Write("\n  There were {0} Failures to read\n\n",fr.NumFailures);
    }
  }
#endif
}
