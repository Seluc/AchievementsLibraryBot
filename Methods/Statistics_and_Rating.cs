using Dapper;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using CourseProjectBot.Models;
using static CourseProjectBot.Common.Common;

namespace CourseProjectBot.Methods {
  class Statistics_and_Rating {
    public static async void ShowStatistics(ITelegramBotClient botClient, Message message) {
      var user = new Models.User();
      using (var conn = new SqlConnection(ConStr)) {
        user = conn.Query<Models.User>("select * from Users where TG_Id = @TG_Id",
          new { TG_Id = message.From.Id }).ToList()[0];
      }

      var msg_ach = "Вы не получили ещё ни одного достижения.";
      var cnt_ach_One = 0;
      var cnt_ach_Two = 0;
      var cnt_ach_Three = 0;
      if (user.Achievements_List.Length > 0) {
        msg_ach = "";

        var achievementsIds = user.GetAchievements_List();
        List<Achievement> achievements = new List<Achievement>();
        foreach (var item in achievementsIds) {
          var ach = new Achievement();
          using (var conn = new SqlConnection(ConStr)) {
            ach = conn.Query<Achievement>("select * from Achievements where Id = @Id",
              new { Id = item }).ToList()[0];
          }

          if (ach.Difficulty == 1) ++cnt_ach_One;
          else if (ach.Difficulty == 2) ++cnt_ach_Two;
          else ++cnt_ach_Three;

          achievements.Add(ach);
        }
        achievements.Sort((a, b) => {
          var comp = a.Game.CompareTo(b.Game);
          if (comp != 0) return comp;
          else return a.Difficulty.CompareTo(b.Difficulty);
        });

        List<int> gamesIds = new List<int>();
        foreach (var item in achievements) {
          var difficulty = "";
          if (item.Difficulty == 1) difficulty = "Бронзовый";
          else if (item.Difficulty == 2) difficulty = "Серебряный";
          else if (item.Difficulty == 3) difficulty = "Золотой";

          if (!gamesIds.Contains(item.Game)) {
            var gameName = "";
            using (var conn = new SqlConnection(ConStr)) {
              gameName = conn.Query<string>("select Name from Games where Id = @Id",
                new { Id = item.Game }).ToList()[0];
            }
            gamesIds.Add(item.Game);

            msg_ach += $"🎮 {gameName}:\n";
          }

          msg_ach += $" 🏆 {difficulty} трофей - {item.Name} - {item.Description}\n\n";
        }
      }

      var msg = $"Имя --> {user.Name}\n" +
                $"Общее количество ачивок --> {cnt_ach_One + cnt_ach_Two + cnt_ach_Three}\n" +
                $"Количество бронзовых ачивок --> {cnt_ach_One}\n" +
                $"Количество серебряных ачивок --> {cnt_ach_Two}\n" +
                $"Количество золотых ачивок --> {cnt_ach_Three}\n" +
                $"Полученные ачивки:\n\n" +
                msg_ach;

      await botClient.SendTextMessageAsync(message.Chat, msg);

      var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
      await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);
    }

    public static async void ShowRating(ITelegramBotClient botClient, Message message) {
      var users = new List<Models.User>();
      using (var conn = new SqlConnection(ConStr)) {
        users = conn.Query<Models.User>("select * from Users where Achievements_List not like \'\'").ToList();
      }

      users.Sort((a, b) => {
        var ach_StrA = "";
        var ach_StrB = "";
        using (var conn = new SqlConnection(ConStr)) {
          ach_StrA = conn.Query<string>("select Achievements_List from Users where TG_Id = @TG_Id",
            new { TG_Id = a.TG_Id }).ToList()[0];
          ach_StrB = conn.Query<string>("select Achievements_List from Users where TG_Id = @TG_Id",
            new { TG_Id = b.TG_Id }).ToList()[0];
        }

        var ach_StrListA = ach_StrA.Split(';');
        var ach_StrListB = ach_StrB.Split(';');
        var ach_ListA = new List<Achievement>();
        var ach_ListB = new List<Achievement>();
        using (var conn = new SqlConnection(ConStr)) {
          foreach (var item in ach_StrListA)
            ach_ListA.Add(conn.Query<Achievement>("select * from Achievements where Id = @Id",
              new { Id = Int32.Parse(item) }).ToList()[0]);

          foreach (var item in ach_StrListB)
            ach_ListB.Add(conn.Query<Achievement>("select * from Achievements where Id = @Id",
              new { Id = Int32.Parse(item) }).ToList()[0]);
        }

        var aTotalPoints = 0;
        var bTotalPoints = 0;
        foreach (var item in ach_ListA) {
          if (item.Difficulty == 1) aTotalPoints += 1;
          else if (item.Difficulty == 2) aTotalPoints += 2;
          else aTotalPoints += 3;
        }
        foreach (var item in ach_ListB) {
          if (item.Difficulty == 1) bTotalPoints += 1;
          else if (item.Difficulty == 2) bTotalPoints += 2;
          else bTotalPoints += 3;
        }

        var comp = bTotalPoints.CompareTo(aTotalPoints);
        if (comp == 0) return b.Id.CompareTo(a.Id);
        return comp;
      });

      var msg = "Топ пользователей:\n";
      msg += $"🥇 {users[0].Name}\n";
      if (users.Count > 1) {
        msg += $"🥈 {users[1].Name}\n";
        msg += $"🥉 {users[2].Name}\n";

        msg += '\n';

        for (int i = 3; i < (users.Count > 10 ? 10 : users.Count); ++i)
          msg += $" {i + 1}  {users[i].Name}\n";
      }

      await botClient.SendTextMessageAsync(message.Chat, msg);
    }
  }
}
