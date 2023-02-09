using Dapper;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using static CourseProjectBot.Common.Common;

namespace CourseProjectBot.Methods {
  public static class Start_and_Register {
    public static async void Start(ITelegramBotClient botClient, Message message) {
      if (!isRegistered(message)) {
        var keyReg = new KeyboardButton("✔️ Зарегистрироваться");
        var keyboard = new ReplyKeyboardMarkup(keyReg) { ResizeKeyboard = true };
        await botClient.SendTextMessageAsync(message.Chat, "Привет! Перед тем как начать пользоваться этим ботом ты должен зарегистрироваться.", replyMarkup: keyboard);

        return;
      } else {
        using (var conn = new SqlConnection(ConStr)) {
          conn.Execute("update Users set [Curr_Menu] = @Curr_Menu where TG_Id = @TG_Id",
            new { Curr_Menu = ((int)MenuIDs.MainMenu), TG_Id = message.From.Id });
        }

        await botClient.SendTextMessageAsync(message.Chat, $"Добро пожаловать, {message.From.FirstName} {message.From.LastName}");

        var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
        await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);

        return;
      }
    }

    public static async void Register(ITelegramBotClient botClient, Message message) {
      using (var conn = new SqlConnection(ConStr)) {
        conn.Execute("pUserAdd @TG_Id, @Name, @Achievements_List, @Curr_Menu",
          new { TG_Id = message.From.Id, Name = message.From.FirstName, Achievements_List = "", Curr_Menu = 0 });
      }

      await botClient.SendTextMessageAsync(message.Chat, "Регистрация прошла успешно!");

      var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
      await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);
    }

    public static bool isRegistered(Message message) {
      var lst = new List<Models.User>();
      using (var conn = new SqlConnection(ConStr)) {
        lst = conn.Query<Models.User>("select * from Users where TG_Id = @TG_Id",
        new { TG_Id = message.From.Id }).ToList();
      }

      if (lst.Count > 0) return true;
      return false;
    }
  }
}
