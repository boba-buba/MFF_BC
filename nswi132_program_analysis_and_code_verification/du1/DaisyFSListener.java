//package gov.nasa.jpf.listener;
//
//import gov.nasa.jpf.vm.*;
//import gov.nasa.jpf.ListenerAdapter;
//
//import java.util.*;
//
//// write to file -> neco
//// read from file -> neco
//// po deleteu a createu
//public class DaisyFSListener extends ListenerAdapter {
//
//    private Map<String, List<String>> directoryFileMap = new HashMap<String, List<String>>();
//
//    @Override
//    public void methodEntered(VM vm, ThreadInfo currentThread, MethodInfo enteredMethod) {
//
//        if (enteredMethod.getName().equals("creat") && enteredMethod.getClassName().contains("DaisyDir")) {
//            // creat(FileHandle dir, byte[] name, FileHandle fh)
//            Object[] args = currentThread.getTopFrame().getArgumentValues(currentThread);
//
//            String dir =  args[0].toString();
//            String filename =  args[1].toString();
//            String fh = args[2].toString();
//
//            // Check precondition: dir != null && fh != null && dir != fh
//            if (dir == null || fh == null || dir.equals(fh)) {
//                vm.getSystemState().setIgnored(true);
//            }
//
//            if (!directoryFileMap.containsKey(dir)) {
//                directoryFileMap.put(dir, new ArrayList<String>());
//            }
//            directoryFileMap.get(dir).add(filename);
//
//        }
//        //DaisyDir.write(fh, 0, 5, DaisyDir.stringToBytes("hello " + Thread.currentThread().getId()));
//        if (enteredMethod.getName().equals("write")) {
//            Object[] args = currentThread.getTopFrame().getArgumentValues(currentThread);
//
//            String dir =  args[0].toString();
//            String filename =  args[1].toString();
//            String fh = args[2].toString();
//        }
//    }
//
//    //@Override
//    public void methodExited(VM vm, ThreadInfo ti, Instruction instruction) {
//        MethodInfo mi = instruction.getMethodInfo();
//        int returnValue = -1;
//
//        if (mi.getName().equals("creat") && mi.getClassName().contains("DaisyDir")) {
//            StackFrame sf = ti.getTopFrame();
//            if (sf != null) {
//                returnValue = sf.peek();
//                System.out.println("Return value: " + returnValue);
//            }
//
//            // Check if file was successfully created
//            if (returnValue == 0) {
//                Object[] args = ti.getTopFrame().getArgumentValues(ti);
//
////                FileHandle fh = (FileHandle) args[2];
////                FileHandle dir = (FileHandle) args[0];
////                byte[] filename = (byte[]) args[1];
////
////                // Ensure the file exists after creation
////                FileHandle lookupFH = new FileHandle();
////                int lookupResult = DaisyDir.lookup(dir, filename, lookupFH);
////                if (lookupResult == -2) {
////                    System.err.println("ERROR: Lookup failed after successful file creation!");
////                    vm.getSystemState().setIgnored(true);
////                }
//            }
//        }
//
//        if (mi.getName().equals("unlink")) {
////            Object[] args = ti.getTopFrame().getArgumentValues(ti);
////            FileHandle dir = (FileHandle) args[0];
////            byte[] filename = (byte[]) args[1];
////
////            FileHandle checkFH = new FileHandle();
////            int lookupResult = DaisyDir.lookup(dir, filename, checkFH);
////            if (lookupResult != -2) {
////                System.err.println("ERROR: File still exists after unlink operation!");
////                vm.getSystemState().setIgnored(true);
//  //          }
//        }
//    }
//
//
//}
