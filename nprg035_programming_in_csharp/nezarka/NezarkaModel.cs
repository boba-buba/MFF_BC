using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace NezarkaBookstore
{
    //
    // Model
    //

    class ModelStore
    {
        private List<Book> books = new List<Book>();
        private List<Customer> customers = new List<Customer>();

        public IList<Book> GetBooks()
        {
            return books;
        }

        public Book GetBook(int id)
        {
            return books.Find(b => b.Id == id);
        }

        public Customer GetCustomer(int id)
        {
            return customers.Find(c => c.Id == id);
        }

        public static ModelStore LoadFrom(TextReader reader)
        {
            var store = new ModelStore();

            try
            {
                if (reader.ReadLine() != "DATA-BEGIN")
                {
                    return null;
                }
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        return null;
                    }
                    else if (line == "DATA-END")
                    {
                        break;
                    }

                    string[] tokens = line.Split(';');
                    switch (tokens[0])
                    {
                        case "BOOK":
                            store.books.Add(new Book
                            {
                                Id = int.Parse(tokens[1]),
                                Title = tokens[2],
                                Author = tokens[3],
                                Price = decimal.Parse(tokens[4])
                            });
                            break;
                        case "CUSTOMER":
                            store.customers.Add(new Customer
                            {
                                Id = int.Parse(tokens[1]),
                                FirstName = tokens[2],
                                LastName = tokens[3]
                            });
                            break;
                        case "CART-ITEM":
                            var customer = store.GetCustomer(int.Parse(tokens[1]));
                            if (customer == null)
                            {
                                return null;
                            }
                            customer.ShoppingCart.Items.Add(new ShoppingCartItem
                            {
                                BookId = int.Parse(tokens[2]),
                                Count = int.Parse(tokens[3])
                            });
                            break;
                        default:
                            return null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is IndexOutOfRangeException)
                {
                    return null;
                }
                throw;
            }

            return store;
        }

        public void AddNewItemInCart(int bookId, Customer customer)
        {
            
            if (customer == null) throw new Exception();
            Book book = GetBook(bookId);
            if (book == null) throw new Exception();
            int index = customer.ShoppingCart.GetItemInCart(bookId);
            

            if ( index == -1 )
            {
                ShoppingCartItem newItem = new ShoppingCartItem();

                customer.ShoppingCart.Items.Add(new ShoppingCartItem
                {
                    BookId = bookId,
                    Count = 1
                });
                
            }
            else
            {
                customer.ShoppingCart.Items[index].Count++;
            }
        }

        public void RemoveItemFromCart(int bookId, Customer customer)
        {
            if (customer == null) throw new Exception();           
            int index = customer.ShoppingCart.GetItemInCart(bookId);
            if (index == -1) throw new Exception();

            int count = customer.ShoppingCart.Items[index].Count;
            if (count == 1)
            {
                customer.ShoppingCart.Items.RemoveAt(index);
            }
            else
            {
                customer.ShoppingCart.Items[index].Count--;
            }

        }
    }

    class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
    }

    class Customer
    {
        private ShoppingCart shoppingCart;

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ShoppingCart ShoppingCart
        {
            get
            {
                if (shoppingCart == null)
                {
                    shoppingCart = new ShoppingCart();
                }
                return shoppingCart;
            }
            set
            {
                shoppingCart = value;
            }
        }
    }

    class ShoppingCartItem
    {
        public int BookId { get; set; }
        public int Count { get; set; }
    }

    class ShoppingCart
    {
        public int CustomerId { get; set; }
        public List<ShoppingCartItem> Items = new List<ShoppingCartItem>();

        public int GetItemInCart(int bookId)
        {
            for (int i = 0; i < Items.Count; i++) 
            {
                if (Items[i].BookId == bookId)
                    return i;
            }
            return -1;
        }


    }
}
