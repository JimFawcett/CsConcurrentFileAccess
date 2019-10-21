///////////////////////////////////////////////////////////////////////
// FileWriter.cs - Handle concurrent write accesses to the same file //
//                                                                   //
// Language:    C#, ver 1.1                                          //
// Platform:    Dell Dimension 8300, Windows XP Pro, SP1             //
// Application: Prototype for Project #5, CSE681, Summer 2004        //
// Author:      Jim Fawcett, Syracuse University                     //
//              jfawcett@twcny.rr.com, (315) 443-3948                //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * This module supports safe concurrent file access for writing while
 * the file may already be open for reading or writing.
 *
 * Public Interface
 * ================
 * FileWriter fw = new FileWriter();
 * bool ok = fw.Open(fileSpec);
 * fw.Close();
 * int NumBytesWritten = fw.CopyFile(ReadPath, WritePath);
 * int NumAttempts = fw.NumAttempts;
 * int NumFailures = fw.NumFailures;
 * fw.SleepMilliSecs = 100;
 */
/*
 * Build Process
 * =============
 * csc /d:TEST_FILEWRITER FileWriter.cs
 * 
 * Maintenance History
 * =================== 
 * ver 1.1 : 17 Nov 04
 * - cosmetic additions and changes to these comments
 * ver 1.0 : 12 Oct 03
 * - first release
 */

//
using System;
using System.IO;
using System.Threading;

namespace FileLocks
{
  class FileWriter
  {
    private FileStream fsw = null;
    int _NumAttempts = 50;
    int _NumFails = 0;
    int _SleepMilliSecs = 50;

    //----< open or create file for writing >--------------------------

    public bool Open(string outFile)
    {
      bool retVal = false;
      for(int j=0; j<_NumAttempts; ++j)
      {
        try
        {
          fsw = File.Open(outFile,FileMode.OpenOrCreate,FileAccess.Write,FileShare.None);
          retVal = true;
          break;
        }
        catch(Exception) 
        {
          Thread.Sleep(_SleepMilliSecs);
          retVal = false; 
        }
      }
      if(retVal == false)
        ++_NumFails;
      return retVal;
    }
    //----< flush stream and close file >------------------------------

    public void Close()
    {
      try
      {
        fsw.Flush();
        fsw.Close();
      }
      catch
      {
        // stream not initialized
      }
    }
    //
    //----< copy from readPath to writePath >--------------------------
    //
    //  Will create writePath file if it does not already exist
    //
    public int CopyFile(string readPath, string writePath)
    {
      int fileSize = 0;
      FileReader fr = new FileReader();
      if(fr.ReadFile(readPath) > 0)
      {
        if(Open(writePath))
        {
          fsw.Write(fr.LastFileRead(),0,fr.LastFileRead().Length);
          Close();
        }
      }
      fr.Close();
      return fr.LastFileRead().Length;
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

#if(TEST_FILEWRITER)

  class TestFileWriter
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
      FileWriter fw = new FileWriter();
      TestFileWriter tfw = new TestFileWriter();

      Console.Write(tfw.Title("Test File Locking"));
      if(args.Length < 2)
      {
        Console.Write("\n  Please enter name of file to read and write");
        return;
      }
      string writePath = Path.GetFullPath("../../..");
      writePath += "/" + args[1];
      string readPath = Path.GetFullPath("../../..");
      readPath += "/" + args[0];

      const int NumCycles =25000;
      int size = 0;
      for(int i=0; i<NumCycles; ++i)
      {
        size = fw.CopyFile(readPath,writePath);
        if(size == 0)
          continue;
        Console.Write("\n  {0,5}: Copied {1} bytes from {2} to {3}",i,size,args[0],args[1]);
      }
      Console.Write("\n  There were {0} failures to copy\n\n",fw.NumFailures);
    }
  }
#endif
}
