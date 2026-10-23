using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
namespace KooliProjekt.Application.Data;
public static class SeedData
{
    public static void Generate(ApplicationDbContext context)
    {
        using var transaction = context.Database.BeginTransaction();
        var firstNames = new[] { "mari", "jaan", "kati", "mart", "liis", "karl", "anna" };
        var lastNames = new[] { "tamm", "saar", "sepp", "kask", "kukk" };
        var topics = new[] { "Veebirakenduse arendus", "Andmebaaside praktikum", "Programmeerimise kodutööd",
            "Rühmaprojekti korraldus", "Praktikaks valmistumine", "Koolinädala plaan", "Portfoolio täiendamine" };
        var tasks = new[] { "Vaata üle ülesande nõuded", "Koosta nädala tööplaan",
            "Loo andmemudeli kavand", "Teosta põhifunktsionaalsus", "Kontrolli lahendust näidisandmetega",
            "Paranda ülevaatusel leitud vead", "Täienda kasutusjuhendit" };
        for (var i = 0; i < 35; i++)
        {
            var userName = firstNames[i % 7] + "." + lastNames[i / 7];
            var user = context.Users.SingleOrDefault(user => user.UserName == userName);
            if (user == null)
            {
                user = new User { UserName = userName, PasswordHash = CreatePasswordHash() };
                context.Users.Add(user);
                context.SaveChanges();
            }
            var title = topics[i % 7] + " – " + userName;
            var list = context.TodoLists.Include(list => list.Items)
                .SingleOrDefault(list => list.UserId == user.Id && list.Title == title);
            if (list == null)
            {
                list = new TodoList { Title = title, UserId = user.Id };
                context.TodoLists.Add(list);
            }
            for (var j = 0; j < tasks.Length; j++)
                if (!list.Items.Any(item => item.Title == tasks[j]))
                    list.Items.Add(new TodoItem { Title = tasks[j], IsCompleted = j < i % 8 });
            context.SaveChanges();
        }
        transaction.Commit();
    }
    private static string CreatePasswordHash()
    {
        // Juhuslikud näidissalasõnad; autentimine ei kuulu selle töö skoopi.
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(RandomNumberGenerator.GetBytes(32), salt,
            100000, HashAlgorithmName.SHA256, 32);
        return "PBKDF2-SHA256:100000:" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }
}
