using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

namespace NezarkaBookstore
{ 
    class Viewer
    {
        ModelStore modelStore;
        public Viewer(ModelStore modelStore) 
        {
            this.modelStore = modelStore;
        }

        string separator = "====";
        string head = 
            """
            <!DOCTYPE html>
            <html lang="en" xmlns="http://www.w3.org/1999/xhtml">
            <head>
                <meta charset="utf-8" />
                <title>Nezarka.net: Online Shopping for Books</title>
            </head>
            <body>
                <style type="text/css">
                    table, th, td {
                        border: 1px solid black;
                        border-collapse: collapse;
                    }
                    table {
                        margin-bottom: 10px;
                    }
                    pre {
                        line-height: 70%;
                    }
                </style>
                <h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>
            """;
        string bottom = """
            </body>
            </html>
            """;
        int BooksTableColumns = 3;


        public void ShowInvalidRequest()
        {
            string ErrorPage = """
                <!DOCTYPE html>
                <html lang="en" xmlns="http://www.w3.org/1999/xhtml">
                <head>
                    <meta charset="utf-8" />
                    <title>Nezarka.net: Online Shopping for Books</title>
                </head>
                <body>
                <p>Invalid request.</p>
                </body>
                </html>
                """;
            Console.WriteLine(ErrorPage);
            Console.WriteLine(separator);

        }

        void ShowMenu(string name, int cartCount)
        {
            string nameString = String.Format("    {0}, here is your menu:", name);
            string cartString = String.Format("            <td><a href=\"/ShoppingCart\">Cart ({0})</a></td>", cartCount);
            Console.WriteLine(nameString);
            Console.WriteLine("    <table>");
            Console.WriteLine("        <tr>");
            Console.WriteLine("            <td><a href=\"/Books\">Books</a></td>");
            Console.WriteLine(cartString);
            Console.WriteLine("        </tr>");
            Console.WriteLine("    </table>");
        }

        void BookCell(Book book)
        {
            Console.WriteLine(String.Format("            <td style=\"padding: 10px;\">"));
            Console.WriteLine(String.Format("                <a href=\"/Books/Detail/{0}\">{1}</a><br />", book.Id, book.Title));
            Console.WriteLine(String.Format("                Author: {0}<br />", book.Author));
            Console.WriteLine(String.Format("                Price: {0} EUR &lt;<a href=\"/ShoppingCart/Add/{1}\">Buy</a>&gt;", book.Price, book.Id ));
            Console.WriteLine(String.Format("            </td>"));
        }
        public void ShowBooksPage(string CustName, int CasrtItemsCount, List<Book> books)
        {
            Console.WriteLine(head);
            ShowMenu(CustName, CasrtItemsCount);
            Console.WriteLine(String.Format("    Our books for you:"));
            Console.WriteLine(String.Format("    <table>"));
            for (int i = 0; i < books.Count; i++) 
            {
                if (i % BooksTableColumns == 0) Console.WriteLine(String.Format("        <tr>"));

                BookCell(books[i]);

                if ((i + 1) % BooksTableColumns == 0) Console.WriteLine(String.Format("        </tr>"));
            
            }
            if (books.Count % BooksTableColumns != 0) Console.WriteLine(String.Format("        </tr>"));
            Console.WriteLine(String.Format("    </table>"));
            Console.WriteLine(bottom);
            Console.WriteLine(separator);
        }
        public void ShowBookDetailsPage(string CustName, int CartItemsCount, Book book)
        {
            Console.WriteLine(head);
            ShowMenu(CustName, CartItemsCount);
            Console.WriteLine("    Book details:");
            Console.WriteLine(String.Format("    <h2>{0}</h2>", book.Title));
            Console.WriteLine("    <p style=\"margin-left: 20px\">");
            Console.WriteLine(String.Format("    Author: {0}<br />", book.Author));
            Console.WriteLine(String.Format("    Price: {0} EUR<br />", book.Price));
            Console.WriteLine("    </p>");
            Console.WriteLine(String.Format("    <h3>&lt;<a href=\"/ShoppingCart/Add/{0}\">Buy this book</a>&gt;</h3>", book.Id));
            Console.WriteLine(bottom);
            Console.WriteLine(separator);
        }

        int CartItemCell(ShoppingCartItem item, Book book)
        {
            int sumPrice = 0;
            Console.WriteLine("        <tr>");
            Console.WriteLine(String.Format("            <td><a href=\"/Books/Detail/{0}\">{1}</a></td>", item.BookId, book.Title));
            Console.WriteLine(String.Format("            <td>{0}</td>", item.Count));
            sumPrice = (int)(item.Count * book.Price);
            if (item.Count > 1) 
            {
                
                Console.WriteLine(String.Format("            <td>{0} * {1} = {2} EUR</td>", item.Count, book.Price, sumPrice));
            }
            else
            {
                Console.WriteLine(String.Format("            <td>{0} EUR</td>", book.Price));
            }
            Console.WriteLine(String.Format("            <td>&lt;<a href=\"/ShoppingCart/Remove/{0}\">Remove</a>&gt;</td>", item.BookId));
            Console.WriteLine("        </tr>");
            return sumPrice;
        }
        void ShowCartItemsTable(ShoppingCart shoppingCart)
        {
            int overallPrice = 0;
            Console.WriteLine("    <table>");
            Console.WriteLine("        <tr>");
            Console.WriteLine("            <th>Title</th>");
            Console.WriteLine("            <th>Count</th>");
            Console.WriteLine("            <th>Price</th>");
            Console.WriteLine("            <th>Actions</th>");
            Console.WriteLine("        </tr>");
            
            for (int i = 0; i < shoppingCart.Items.Count; i++) 
            {
                Book book = modelStore.GetBook(shoppingCart.Items[i].BookId);
                overallPrice += CartItemCell(shoppingCart.Items[i], book);
            }

            Console.WriteLine("    </table>");
            Console.WriteLine(String.Format("    Total price of all items: {0} EUR", overallPrice));

        }
        public void ShowShoppingCartPage(string CustName, int CartItemsCount, ShoppingCart shoppingCart) 
        {
            Console.WriteLine(head);
            ShowMenu(CustName, CartItemsCount);
            if (CartItemsCount == 0)
            {
                Console.WriteLine("    Your shopping cart is EMPTY.");
                
            } else
            {
                Console.WriteLine("    Your shopping cart:");
                ShowCartItemsTable(shoppingCart);
                
            }

            Console.WriteLine(bottom);
            Console.WriteLine(separator);
        }
    }

}