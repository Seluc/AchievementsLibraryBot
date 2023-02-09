using Dapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

using static CourseProjectBot.Common.Common;
using static CourseProjectBot.Methods.AddAchievement;
using static CourseProjectBot.Methods.Start_and_Register;
using static CourseProjectBot.Methods.Statistics_and_Rating;

namespace CourseProjectBot {
  class Program {
    static ITelegramBotClient Bot = new TelegramBotClient("5406853321:AAE5A5Xjw7owfJDrtX33VCp7XflK0CeRaCk");


    static void Main(string[] args) {
      Console.WriteLine("Запущен бот " + Bot.GetMeAsync().Result.FirstName);

      var cts = new CancellationTokenSource();
      var cancellationToken = cts.Token;
      var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
      Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
      Console.ReadLine();
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
      Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
      if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message) {

        if (update.Message.Text == null) {
          await botClient.SendTextMessageAsync(update.Message.Chat, "Извините, я вас не понял. Прошу не отправлять ничего кроме текстовых сообщений.");
          return;
        }

        Console.WriteLine("\n\n" + update.Message.From.FirstName + " --> " + update.Message.Text + "\n\n");

        var message = update.Message;
        switch (message.Text.ToLower()) {
          case "/start": Start(botClient, message); break;

          case "✔️ зарегистрироваться":
          case "зарегистрироваться": {
              if (!isRegistered(message)) Register(botClient, message);
              else await botClient.SendTextMessageAsync(update.Message.Chat, "Вы уже зарегистрированы.");
            }
            break;

          case "📝 показать мою статистику":
          case "показать мою статистику": {
              if (isRegistered(message)) ShowStatistics(botClient, message);
              else await botClient.SendTextMessageAsync(update.Message.Chat, "Вы не зарегистрированы. Для регистрации напишите \"Зарегистрироваться\"");
            }
            break;

          case "🏆 отметить выполненые достижения":
          case "отметить выполненые достижения": {
              if (isRegistered(message)) SearchGames(botClient, message);
              else await botClient.SendTextMessageAsync(update.Message.Chat, "Вы не зарегистрированы. Для регистрации напишите \"Зарегистрироваться\"");
            }
            break;

          case "🥇 рейтинг пользователей":
          case "рейтинг пользователей": {
              if (isRegistered(message)) ShowRating(botClient, message);
              else await botClient.SendTextMessageAsync(update.Message.Chat, "Вы не зарегистрированы. Для регистрации напишите \"Зарегистрироваться\"");
            }
            break;

          case "🔙 назад":
          case "назад": {
              if (isRegistered(message)) {
                var currMenu = 0;
                using (var conn = new SqlConnection(ConStr)) {
                  currMenu = conn.Query<int>("select Curr_Menu from Users where TG_Id = @TG_Id",
                    new { TG_Id = message.From.Id }).ToList()[0];
                }
                switch (currMenu) {
                  case 1: {
                      currMenu = ((int)MenuIDs.MainMenu);
                      var keyboard = new ReplyKeyboardMarkup(MenuKeys) { ResizeKeyboard = true };
                      await botClient.SendTextMessageAsync(message.Chat, "Выбери действие.", replyMarkup: keyboard);
                    }
                    break;
                  case 2: SearchGames(botClient, message); break;
                  case 3: SearchGames(botClient, message); break;
                  default: await botClient.SendTextMessageAsync(message.Chat, "Куда назад то? Дальше уже некуда."); break;
                }
              } else await botClient.SendTextMessageAsync(update.Message.Chat, "Вы не зарегистрированы. Для регистрации напишите \"Зарегистрироваться\"");
            }
            break;

          default: {
              if (isRegistered(message)) {
                var currMenu = 0;
                using (var conn = new SqlConnection(ConStr)) {
                  currMenu = conn.Query<int>("select Curr_Menu from Users where TG_Id = @TG_Id",
                    new { TG_Id = message.From.Id }).ToList()[0];
                }
                switch (currMenu) {
                  case ((int)MenuIDs.Searching): ShowGames(botClient, message); break;
                  case ((int)MenuIDs.ChoosingGame): ShowAchievements(botClient, message); break;
                  case ((int)MenuIDs.ChoosingAchievement): AddAchievements(botClient, message); break;
                  default: await botClient.SendTextMessageAsync(message.Chat, "Извините, я вас не понял."); break;
                }
              } else await botClient.SendTextMessageAsync(update.Message.Chat, "Извините, я вас не понял.");
            }
            break;
        }
      }
    }

    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
  }
}
