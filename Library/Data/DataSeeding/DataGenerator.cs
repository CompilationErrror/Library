using DataModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data.DataSeeding
{
    public static class DataGenerator
    {
        public static List<Genre> GenerateSeedGenres()
        {
            return new List<Genre>
            {
                new Genre { Name = "Fiction" }, new Genre { Name = "Non-Fiction" }, new Genre { Name = "Mystery" }, new Genre { Name = "Thriller" },
                new Genre { Name = "Science Fiction" }, new Genre { Name = "Fantasy" }, new Genre { Name = "Romance" }, new Genre { Name = "Historical Fiction" },
                new Genre { Name = "Horror" }, new Genre { Name = "Biography" }, new Genre { Name = "Autobiography" }, new Genre { Name = "Self-Help" },
                new Genre { Name = "Business" }, new Genre { Name = "Poetry" }, new Genre { Name = "Drama" }, new Genre { Name = "Adventure" },
                new Genre { Name = "Young Adult" }, new Genre { Name = "Children's Literature" }, new Genre { Name = "Graphic Novel" }, new Genre { Name = "Crime" },
                new Genre { Name = "Dystopian" }, new Genre { Name = "Memoir" }, new Genre { Name = "Travel" },new Genre { Name = "Cookbook" },
                new Genre { Name = "Philosophy" }, new Genre { Name = "Psychology" }, new Genre { Name = "Science" }, new Genre { Name = "History" },
                new Genre { Name = "Religion" }, new Genre { Name = "True Crime" }
            };
        }

        public static List<Book> GenerateSeedBooks(List<Genre> genres)
        {
            var genreDict = genres.ToDictionary(g => g.Name, g => g.Id);
            var books = new List<Book>();

            var genreAssignments = new Dictionary<string, string>
            {
                { "The Great Gatsby", "Fiction" }, { "To Kill a Mockingbird", "Fiction" }, { "1984", "Science Fiction" }, { "Pride and Prejudice", "Romance" },
                { "The Catcher in the Rye", "Fiction" }, { "The Hobbit", "Fantasy" }, { "Fahrenheit 451", "Dystopian" }, { "Brave New World", "Science Fiction" },
                { "Moby Dick", "Fiction" }, { "War and Peace", "Historical Fiction" }, { "Crime and Punishment", "Drama" }, { "The Odyssey", "Drama" },
                { "Jane Eyre", "Romance" }, { "Wuthering Heights", "Romance" }, { "Lord of the Flies", "Young Adult" }, { "Animal Farm", "Fiction" },
                { "Catch-22", "Fiction" }, { "The Grapes of Wrath", "Historical Fiction" }, { "The Old Man and the Sea", "Fiction" }, { "Ulysses", "Fiction" },
                { "The Divine Comedy", "Poetry" }, { "Don Quixote", "Adventure" }, { "Frankenstein", "Horror" }, { "Dracula", "Horror" }, 
                { "The Count of Monte Cristo", "Adventure" }, { "Les Misérables", "Historical Fiction" }, { "Great Expectations", "Fiction" },
                { "A Tale of Two Cities", "Historical Fiction" }, { "Alice's Adventures in Wonderland", "Fantasy" }, { "The Picture of Dorian Gray", "Fiction" },
                { "The Brothers Karamazov", "Drama" }, { "Heart of Darkness", "Fiction" }, { "The Sun Also Rises", "Fiction" }, { "For Whom the Bell Tolls", "Fiction" },
                { "One Hundred Years of Solitude", "Fiction" }, { "Beloved", "Historical Fiction" }, { "Slaughterhouse-Five", "Science Fiction" },
                { "The Handmaid's Tale", "Dystopian" }, { "The Road", "Dystopian" }, { "American Gods", "Fantasy" }, { "Dune", "Science Fiction" },
                { "Neuromancer", "Science Fiction" }, { "Foundation", "Science Fiction" }, { "Ender's Game", "Science Fiction" }, { "A Game of Thrones", "Fantasy" },
                { "The Name of the Wind", "Fantasy" }, { "The Wise Man's Fear", "Fantasy" }, { "Mistborn", "Fantasy" },{ "The Way of Kings", "Fantasy" },
                { "The Hunger Games", "Dystopian" }, { "Harry Potter and the Philosopher's Stone", "Fantasy" }
            };

            var titles = new string[]
            {
                "The Great Gatsby", "To Kill a Mockingbird", "1984", "Pride and Prejudice",
                "The Catcher in the Rye", "The Hobbit", "Fahrenheit 451", "Brave New World",
                "Moby Dick", "War and Peace", "Crime and Punishment", "The Odyssey",
                "Jane Eyre", "Wuthering Heights", "Lord of the Flies", "Animal Farm",
                "Catch-22", "The Grapes of Wrath", "The Old Man and the Sea", "Ulysses",
                "The Divine Comedy", "Don Quixote", "Frankenstein", "Dracula",
                "The Count of Monte Cristo", "Les Misérables", "Great Expectations",
                "A Tale of Two Cities", "Alice's Adventures in Wonderland", "The Picture of Dorian Gray",
                "The Brothers Karamazov", "Heart of Darkness", "The Sun Also Rises",
                "For Whom the Bell Tolls", "One Hundred Years of Solitude", "Beloved",
                "Slaughterhouse-Five", "The Handmaid's Tale", "The Road", "American Gods",
                "Dune", "Neuromancer", "Foundation", "Ender's Game", "A Game of Thrones",
                "The Name of the Wind", "The Wise Man's Fear", "Mistborn", "The Way of Kings",
                "The Hunger Games", "Harry Potter and the Philosopher's Stone"
            };

            var authors = new string[]
            {
                "F. Scott Fitzgerald", "Harper Lee", "George Orwell", "Jane Austen",
                "J.D. Salinger", "J.R.R. Tolkien", "Ray Bradbury", "Aldous Huxley",
                "Herman Melville", "Leo Tolstoy", "Fyodor Dostoevsky", "Homer",
                "Charlotte Brontë", "Emily Brontë", "William Golding", "George Orwell",
                "Joseph Heller", "John Steinbeck", "Ernest Hemingway", "James Joyce",
                "Dante Alighieri", "Miguel de Cervantes", "Mary Shelley", "Bram Stoker",
                "Alexandre Dumas", "Victor Hugo", "Charles Dickens", "Charles Dickens",
                "Lewis Carroll", "Oscar Wilde", "Fyodor Dostoevsky", "Joseph Conrad",
                "Ernest Hemingway", "Ernest Hemingway", "Gabriel García Márquez",
                "Toni Morrison", "Kurt Vonnegut", "Margaret Atwood", "Cormac McCarthy",
                "Neil Gaiman", "Frank Herbert", "William Gibson", "Isaac Asimov",
                "Orson Scott Card", "George R.R. Martin", "Patrick Rothfuss",
                "Patrick Rothfuss", "Brandon Sanderson", "Brandon Sanderson",
                "Suzanne Collins", "J.K. Rowling"
            };

            var years = new int[]
            {
                1925, 1960, 1949, 1813,
                1951, 1937, 1953, 1932,
                1851, 1869, 1866, -800,
                1847, 1847, 1954, 1945,
                1961, 1939, 1952, 1922,
                1320, 1605, 1818, 1897,
                1844, 1862, 1861, 1859,
                1865, 1890, 1880, 1899,
                1926, 1940, 1967, 1987,
                1969, 1985, 2006, 2001,
                1965, 1984, 1951, 1985,
                1996, 2007, 2011, 2006,
                2010, 2008, 1997
            };

            var random = new Random();

            for (int i = 0; i < titles.Length; i++)
            {
                double price = Math.Round(random.NextDouble() * 100, 2);

                if (price < 10)
                {
                    i--;
                    continue;
                }

                var book = new Book
                {
                    Title = titles[i],
                    Author = authors[i],
                    PublishedYear = years[i],
                    QuantityInStock = random.Next(1, 10),
                    Price = price,
                };

                if (genreAssignments.TryGetValue(book.Title, out string genreName))
                {
                    if (genreDict.TryGetValue(genreName, out int genreId))
                    {
                        book.GenreId = genreId;
                    }
                }

                books.Add(book);
            }

            return books;
        }

        public static List<User> GenerateSeedUsers()
        {
            var users = new List<User>();

            // Admin user(Seeding admin depend on how do we grant admin role)
            //users.Add(new User
            //{
            //    Name = "Admin",
            //    Surname = "User",
            //    Email = "admin@library.com",
            //    Username = "admin",
            //    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            //    IsAdmin = true,
            //    TotalBooksReturned = 0
            //});

            var userList = new[]
            {
                new { Name = "John", Surname = "Smith", Username = "jsmith", Email = "john.smith@email.com", Returned = 15 },
                new { Name = "Emma", Surname = "Johnson", Username = "ejohnson", Email = "emma.johnson@email.com", Returned = 23 },
                new { Name = "Michael", Surname = "Williams", Username = "mwilliams", Email = "michael.williams@email.com", Returned = 8 },
                new { Name = "Sarah", Surname = "Brown", Username = "sbrown", Email = "sarah.brown@email.com", Returned = 31 },
                new { Name = "David", Surname = "Jones", Username = "djones", Email = "david.jones@email.com", Returned = 12 },
                new { Name = "Lisa", Surname = "Garcia", Username = "lgarcia", Email = "lisa.garcia@email.com", Returned = 19 },
                new { Name = "James", Surname = "Martinez", Username = "jmartinez", Email = "james.martinez@email.com", Returned = 6 },
                new { Name = "Maria", Surname = "Rodriguez", Username = "mrodriguez", Email = "maria.rodriguez@email.com", Returned = 27 },
                new { Name = "Robert", Surname = "Miller", Username = "rmiller", Email = "robert.miller@email.com", Returned = 14 },
                new { Name = "Jennifer", Surname = "Davis", Username = "jdavis", Email = "jennifer.davis@email.com", Returned = 9 }
            };

            foreach (var user in userList)
            {
                users.Add(new User
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Username = user.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    IsAdmin = false,
                    TotalBooksReturned = user.Returned
                });
            }

            return users;
        }

        public static async Task<List<OrderedBook>> GenerateSeedOrderedBooksAsync(LibraryContext context)
        {
            var orderedBooks = new List<OrderedBook>();

            var books = await context.Books.ToListAsync();
            var users = await context.Users.Where(u => !u.IsAdmin).ToListAsync();

            if (!books.Any() || !users.Any())
            {
                return orderedBooks;
            }

            var userDict = users.ToDictionary(u => u.Username);

            // Helper to get book by title
            Book GetBook(string title) => books.FirstOrDefault(b => b.Title == title);
            User GetUser(string username) => userDict.ContainsKey(username) ? userDict[username] : null;

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Great Gatsby").Id,
                UserId = GetUser("jsmith").Id,
                OrderDate = DateTime.Now.AddDays(-5),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("1984").Id,
                UserId = GetUser("jsmith").Id,
                OrderDate = DateTime.Now.AddDays(-10),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Hobbit").Id,
                UserId = GetUser("jsmith").Id,
                OrderDate = DateTime.Now.AddDays(-3),
                ReturnDate = null
            });

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Pride and Prejudice").Id,
                UserId = GetUser("ejohnson").Id,
                OrderDate = DateTime.Now.AddDays(-7),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Jane Eyre").Id,
                UserId = GetUser("ejohnson").Id,
                OrderDate = DateTime.Now.AddDays(-8),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Wuthering Heights").Id,
                UserId = GetUser("ejohnson").Id,
                OrderDate = DateTime.Now.AddDays(-2),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("To Kill a Mockingbird").Id,
                UserId = GetUser("ejohnson").Id,
                OrderDate = DateTime.Now.AddDays(-4),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Handmaid's Tale").Id,
                UserId = GetUser("ejohnson").Id,
                OrderDate = DateTime.Now.AddDays(-6),
                ReturnDate = null
            });

            //2 overdue books (borrowed more than 14 days ago, not yet returned)
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Dune").Id,
                UserId = GetUser("mwilliams").Id,
                OrderDate = DateTime.Now.AddDays(-20),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Foundation").Id,
                UserId = GetUser("mwilliams").Id,
                OrderDate = DateTime.Now.AddDays(-25),
                ReturnDate = null
            });

            //4 books, 1 overdue
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Harry Potter and the Philosopher's Stone").Id,
                UserId = GetUser("sbrown").Id,
                OrderDate = DateTime.Now.AddDays(-18),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Hunger Games").Id,
                UserId = GetUser("sbrown").Id,
                OrderDate = DateTime.Now.AddDays(-5),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("A Game of Thrones").Id,
                UserId = GetUser("sbrown").Id,
                OrderDate = DateTime.Now.AddDays(-8),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Name of the Wind").Id,
                UserId = GetUser("sbrown").Id,
                OrderDate = DateTime.Now.AddDays(-1),
                ReturnDate = null
            });

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Neuromancer").Id,
                UserId = GetUser("djones").Id,
                OrderDate = DateTime.Now.AddDays(-12),
                ReturnDate = null
            });

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Beloved").Id,
                UserId = GetUser("lgarcia").Id,
                OrderDate = DateTime.Now.AddDays(-9),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("One Hundred Years of Solitude").Id,
                UserId = GetUser("lgarcia").Id,
                OrderDate = DateTime.Now.AddDays(-4),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Road").Id,
                UserId = GetUser("lgarcia").Id,
                OrderDate = DateTime.Now.AddDays(-2),
                ReturnDate = null
            });

            //2 books, 1 close to being overdue
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Ender's Game").Id,
                UserId = GetUser("jmartinez").Id,
                OrderDate = DateTime.Now.AddDays(-13),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("American Gods").Id,
                UserId = GetUser("jmartinez").Id,
                OrderDate = DateTime.Now.AddDays(-7),
                ReturnDate = null
            });

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("Mistborn").Id,
                UserId = GetUser("mrodriguez").Id,
                OrderDate = DateTime.Now.AddDays(-6),
                ReturnDate = null
            });

            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Way of Kings").Id,
                UserId = GetUser("jdavis").Id,
                OrderDate = DateTime.Now.AddDays(-10),
                ReturnDate = null
            });
            orderedBooks.Add(new OrderedBook
            {
                BookId = GetBook("The Wise Man's Fear").Id,
                UserId = GetUser("jdavis").Id,
                OrderDate = DateTime.Now.AddDays(-3),
                ReturnDate = null
            });

            return orderedBooks;
        }
    }
}
