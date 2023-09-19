using System;
using System.Threading;

// MultiCellBuffer:
//      Comm line between ParkingAgents and ParkingStructures
//      3 Data Cells:
//          each a reference to an Order obj.
//          enforced by semaphore n = 3
//          Locks for read/write permissions between threads
//          CANNOT USE QUEUE
//          can read at the same time
//          no read/write || write/write
//          IF message gen is slow, use sleep func (prob base^2 growth)
//          make sure cells are used as uniformly distributed as possible
//      setters and getters for cells
namespace Parking_Service {
    internal class MultiCellBuffer {
        // 3 thread spots initially available, only 3 allowed. 
        private static int size = 3;

        // Used to allow max number of threads to be able to access this class' cells
        // Semaphores also have a "fairness" trait that acts like a queue (FIFO)
        private Semaphore semaphore = new Semaphore(initialCount: 3, maximumCount: 3);

        // Used to lock when only reading, or writing to prevent out-of-sync reads
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        // Buffer containing the orders from the agents
        private Order[] orders = new Order[size];

        // Complement buffer to allow locking
        private readonly object[] locks = new object[size];
        public void InitializeLocks() {
            for(int i = 0; i < size; i++) {
                locks[i] = new object();
            }
        }
        private const int maxAttempts = 5;
        private const int maxWaitTime = 5000;

        // /////////////////////////////////////////////////////////////////////////////////////
        // Pass one order to be placed in the multi-cell Buffer. This method will attempt 5 
        // times to write into the buffer. If successful, return true, otherwise, return false. 

        public bool SetOneCell(Order order) {

            // Attempt a thread access 5 times
            for (int attempts = 0; attempts < maxAttempts; attempts++) {

                // If a thread spot is not available, use another attempts
                if (!semaphore.WaitOne(maxWaitTime))
                    continue;
                // else the thread has gained access to the order array.

                //Console.WriteLine("\t\tSemaphore obtained");
                try {
                    // Iterate through the MultiCellBuffer
                    for (int j = 0; j < size; j++) {
                        // If this order is not empty, iterate thru other ones. 
                        if (orders[j] != null) continue;

                        // else 
                        lock (locks[j]) {
                            // Console.WriteLine("SetOneCell Locked!");
                            try {
                                orders[j] = order;
                                // Console.WriteLine("Order Inserted!");
                                return true;

                                // catch error in the buffer manipulation. 
                            } catch (Exception e) {
                                LogError("In SetOneCell", e);
                            }
                            finally { 
                                // Console.WriteLine("SetOneCell Unlocked!");
                            }
                        } // Auto release lock
                    }
                    // An empty cell was not found. 
                    return false;
                
                    // catch error in the buffer iteration
                } 
                catch (Exception e) {
                    LogError("In Semaphore aquisition in SetOneCell", e);                // release the semaphore value
                } 
                finally { 
                    semaphore.Release();
                    //Console.WriteLine("\t\tSemaphore released");
                }  
            }
            // semaphore spot was not obtained
            return false;
        }


        // //////////////////////////////////////////////////////////////////////////////////////////
        // Gets an order from the buffer, same as setOneCell logic, but the main operation is to
        // grab an order from the array, clear it in the array and return that order. 
        public Order GetOneCell() {
            

            // attempt to get an order up to 5 times
            for (int attempts = 0; attempts < maxAttempts; attempts++) {

                // If a thread spot is not available, use another attempt
                if (!semaphore.WaitOne(maxWaitTime))
                    continue;

                // else read-lock object and parse thru orders
                //Console.WriteLine("\t\tSemaphore obtained");
                try {

                    for (int j = 0; j < orders.Length; j++) {
                        // if an order is null, move on to other cells
                        if (orders[j] == null)
                            continue;

                        // otherwise process the order. 
                        Order orderToBeProcessed = null;
                        ExtractOrder(ref orderToBeProcessed, j);

                        // if ExtractOrder was successful, return the order extracted. 
                        if (orderToBeProcessed != null)
                            // Console.WriteLine("Order Extracted!");
                            return orderToBeProcessed;
                        
                    }
                }
                catch (Exception e) {
                    LogError("In GetOneCell", e);
                    return null;
                }
                finally {
                    // Always release semaphore to avoid deadlocks. 
                    semaphore.Release();
                    //Console.WriteLine("\t\tSemaphore released");
                }
            }
            // Couldn't get an order :(
            return null;
        }

        // This is a helper method to upgrade the reader lock into a writer lock and extract an order
        // The reason this is another method is because special precaution needs to be taken when 
        // upgrading a lock, as there is a gap that may allow another thread to modify a value. 
        private void ExtractOrder( ref Order orderToBeProcessed, int j) {
            try {
                // Exiting & entering locks gives a gap for other threads to enter and lock
                lock (locks[j]) {
                    // Console.WriteLine("ExtractOrder WriteLocked!");

                    // If the order has not already been extracted, extract it. 
                    if (orders[j] != null) {
                        orderToBeProcessed = orders[j];
                        orders[j] = null;
                    }
                } // auto release lock
            }
            catch (Exception e) {
                LogError("in ExtractOrder", e);
            }
            finally {
                // Console.WriteLine("ExtractOrder WriteUnlocked!");
                // Console.WriteLine("Order in \"ExtractOrder\"");
            }
        }

        //helper function that makes logging try-catch exceptions easier. 
        private static void LogError(string message, Exception e) {
            Console.WriteLine($"Error:: {message} \n::: {e.Message}");
        }

        public void PrintBuffer() {
            Console.Write("Buffer: ");
            for(int i  = 0; i < 3; i++) {
                if (orders[i] != null)
                    Console.Write($"{i} :: {orders[i].GetID()} ");
                else Console.Write($"{i} :: null ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public void DisposeOfSemaphore() {
            semaphore.Dispose();
        }
    }
}
