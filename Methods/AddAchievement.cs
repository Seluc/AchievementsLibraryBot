using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using CourseProjectBot.Models;
using static CourseProjectBot.Common.Common;

namespace CourseProjectBot.Methods {
  class AddAchievement {
    public static async void SearchGames(ITelegramBotClient botClient, Message message) {
      using (var conn = new SqlConnection(ConStr)) {
        conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
          new { Curr_Menu = ((int)MenuIDs.Searching), TG_Id = message.From.Id });
      }


      var keyReg = new KeyboardButton("🔙 Назад");
      var keyboard = new ReplyKeyboardMarkup(keyReg) { ResizeKeyboard = true };
      await botClient.SendTextMessageAsync(message.Chat, "Введите название игры, в которой вы получили достижение.",
        replyMarkup: keyboard);
    }

    public static async void ShowGames(ITelegramBotClient botClient, Message message) {
      using (var conn = new SqlConnection(ConStr)) {
        conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
          new { Curr_Menu = ((int)MenuIDs.ChoosingGame), TG_Id = message.From.Id });
      }

      var games = new List<Models.Game>();
      using (var conn = new SqlConnection(ConStr)) {
        games = conn.Query<Models.Game>("select * from Games where Name like @Name",
          new { Name = $"%{message.Text}%" }).ToList();
      }

      if (games.Count <= 0) {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
          InlineKeyboardButton.WithUrl(text: "Связаться с разработчиками",
          url: @"https://www.youtube.com/watch?v=dQw4w9WgXcQ")
        });
        await botClient.SendTextMessageAsync(message.Chat,
          $"Извините, но введённой вами игры ({message.Text}) нет в нашей базе. Свяжитесь с нами и мы обязательно добавим её!\nExample@example.com",
          replyMarkup: inlineKeyboard);

        SearchGames(botClient, message);

        return;
      } else if (games.Count == 1) ShowAchievements(botClient, new Message() { Chat = message.Chat, From = message.From, Text = $"{games[0].Id }" });
      else {
        var msg = $"Напишите ID игры которую хотите выбрать:\n";
        foreach (var item in games)
          msg += $"{item.Id} - {item.Name}\n";

        await botClient.SendTextMessageAsync(message.Chat, msg);
      }
    }

    public static async void ShowAchievements(ITelegramBotClient botClient, Message message) {
      using (var conn = new SqlConnection(ConStr)) {
        conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
          new { Curr_Menu = ((int)MenuIDs.ChoosingAchievement), TG_Id = message.From.Id });
      }

      var id = 0;
      if (!Int32.TryParse(message.Text, out id)) {
        await botClient.SendTextMessageAsync(message.Chat, $"Введено неверное значение!");
        SearchGames(botClient, message);
        return;
      }

      var ach_List = new List<Achievement>();
      using (var conn = new SqlConnection(ConStr)) {
        ach_List = conn.Query<Achievement>("select * from Achievements where Game = @Game",
          new { Game = id }).ToList();
      }
      if (ach_List.Count <= 0) {
        await botClient.SendTextMessageAsync(message.Chat, $"Введено неверное значение!");
        SearchGames(botClient, message);
        return;
      }

      var gameName = "";
      using (var conn = new SqlConnection(ConStr)) {
        gameName += conn.Query<string>("select name from Games where Id = @Id",
          new { Id = id }).ToList()[0];
      }

      var msg = $"{gameName}\n\nНапишите ID достижения которое выполнили (можете написать несколько IDшек, разделяя их знаком \';\'):\n\n";
      foreach (var item in ach_List) {
        var difficulty = "";
        if (item.Difficulty == 1) difficulty = "Бронзовый";
        else if (item.Difficulty == 2) difficulty = "Серебряный";
        else if (item.Difficulty == 3) difficulty = "Золотой";
        msg += $"{item.Id} - {difficulty} трофей - {item.Name} - {item.Description}\n\n";
      }
      await botClient.SendTextMessageAsync(message.Chat, msg);
    }

    public static async void AddAchievements(ITelegramBotClient botClient, Message message) {
      var num = 0;
      if (Int32.TryParse(message.Text, out num)) {
        var ids = "";
        using (var conn = new SqlConnection(ConStr)) {
          ids += conn.Query<string>("select Achievements_List from Users where TG_Id = @TG_Id",
            new { TG_Id = message.From.Id }).ToList()[0];
        }

        if (ids.Contains($"{num}")) {
          await botClient.SendTextMessageAsync(message.Chat, $"Это достижение уже есть в вашем профиле!");
          SearchGames(botClient, message);
          return;
        } else if (ids.Length > 0) ids += $";{num}";
        else ids += $"{num}";

        using (var conn = new SqlConnection(ConStr)) {
          conn.Execute("update Users set [Achievements_List] = @Achievements_List where TG_Id = @TG_Id",
            new { Achievements_List = ids, TG_Id = message.From.Id });
        }

        await botClient.SendTextMessageAsync(message.Chat, $"Достижение успешно добавлено в ваш профиль!");
        using (var conn = new SqlConnection(ConStr)) {
          conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
            new { Curr_Menu = ((int)MenuIDs.MainMenu), TG_Id = message.From.Id });
        }

        var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
        await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);
      } else {
        var ids_List = message.Text.Split(';').ToList();
        ids_List = ids_List.Distinct().ToList();
        foreach (var item in ids_List) {
          if (!Int32.TryParse(item, out num)) {
            await botClient.SendTextMessageAsync(message.Chat, $"Введенно неверное значение!");
            SearchGames(botClient, message);
            return;
          }
        }

        var ids = "";
        using (var conn = new SqlConnection(ConStr)) {
          ids += conn.Query<string>("select Achievements_List from Users where TG_Id = @TG_Id",
            new { TG_Id = message.From.Id }).ToList()[0];
        }

        foreach (var item in ids_List) {
          if (ids.Contains(item)) {
            await botClient.SendTextMessageAsync(message.Chat, $"Одно или несколько из этих достижений уже есть в вашем профиле!");
            SearchGames(botClient, message);
            return;
          }

          ids += $";{item}";
        }
        while (ids[0] == ';') ids = ids.Remove(0, 1);

        using (var conn = new SqlConnection(ConStr)) {
          conn.Execute("update Users set [Achievements_List] = @Achievements_List where TG_Id = @TG_Id",
            new { Achievements_List = ids, TG_Id = message.From.Id });
        }

        await botClient.SendTextMessageAsync(message.Chat, $"Достижения успешно добавлено в ваш профиль!");
        using (var conn = new SqlConnection(ConStr)) {
          conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
            new { Curr_Menu = ((int)MenuIDs.MainMenu), TG_Id = message.From.Id });
        }

        var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
        await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);
      }
    }
  }
}
