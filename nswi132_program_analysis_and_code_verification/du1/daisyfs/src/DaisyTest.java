//package fse2006daisy;
//
//import fse2006daisy.*;
//import fse2006daisy.daisy.*;
//import fse2006daisy.daisy.DaisyDir;
//import fse2006daisy.daisy.FileHandle;
//import fse2006daisy.daisy.Petal;
//import gov.nasa.jpf.vm.Verify;
//
///**
// * The DaisyTest provides a threaded example that will exercise
// * the Daisy file system.  It will initialize the file system and then create
// * 2 users of the file system.
// *
// * @author Todd Wallentine tcw AT cis ksu edu
// * @version $Revision: 1.1 $ - $Date: 2004/12/15 20:47:45 $
// */
//public class DaisyTest {
//
//	/**
//	 * The main method will initialize the file system by creating a file, initializing it with contents,
//	 * and then starting 2 user threads (one reader and one unliner).
//	 * The main method then waits for the threads to complete and then completes.
//	 *
//	 * @param args Ignored.
//	 */
//	public static void main(String[] args) {
//        Attribute a = new Attribute();
//        FileHandle root = new FileHandle();
//        FileHandle fh = new FileHandle();
//
//		// The inode number of the root directory is always 0.
//        root.inodenum = 0;
//        Petal.init(false);
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//        System.out.println("Creating file named cow in root:");
//		System.out.println("Return code: " +
//		DaisyDir.creat(root, DaisyTest.stringToBytes("cow"), fh));
//        System.out.println("Returned file handle: " + fh);
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//		System.out.println("2) Creating file named cow in root:");
//		System.out.println("Return code: " +
//				DaisyDir.creat(root, DaisyTest.stringToBytes("cow"), fh));
//		System.out.println("Returned file handle: " + fh);
//		System.out.println("Disk contents:");
//		Daisy.dumpDisk();
//
//		System.out.println("Writing the string moo into file cow:");
//        System.out.println("Return code: " +
//		DaisyDir.write(fh, 2, 3, DaisyTest.stringToBytes("moo")));
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//		System.out.println("Deleting the file cow from the root directory:");
//        System.out.println("Return code: " +
//		DaisyDir.unlink(root, DaisyTest.stringToBytes("cow")));
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//		System.out.println("Creating the file wombat in the root directory:");
//        System.out.println("Return code: " +
//		DaisyDir.creat(root, DaisyTest.stringToBytes("wombat"), fh));
//        System.out.println("Returned file handle: " + fh);
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//		System.out.println("Writing the string moomoo into file wombat:");
//        System.out.println("Return code: " +
//		DaisyDir.write(fh, 0, 6, DaisyTest.stringToBytes("moomoo")));
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//
//		System.out.println("Deleting the file cow from the root directory:");
//        System.out.println("Return code: " +
//		DaisyDir.unlink(root, DaisyTest.stringToBytes("cow")));
//		System.out.println("Disk contents:");
//        Daisy.dumpDisk();
//	}
//
//	static byte[] stringToBytes(String s) {
//		byte b[] = new byte[s.length()];
//		for (int i = 0; i < s.length(); i++) {
//			b[i] = (byte) s.charAt(i);
//		}
//		return b;
//	}
//}
