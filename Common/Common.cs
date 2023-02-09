using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace CourseProjectBot.Common {
  public static class Common {
    public enum MenuIDs {
      MainMenu,
      Searching,
      ChoosingGame,
      ChoosingAchievement
    }

    public static string ConStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Seluc\Documents\CourseDB.mdf;Integrated Security=True;Connect Timeout=30";
    
    public static List<List<KeyboardButton>> MenuKeys = new List<List<KeyboardButton>>() {
      new List<KeyboardButton>() {
        new KeyboardButton("📝 Показать мою статистику"),
      },
      new List<KeyboardButton>() {
        new KeyboardButton("🏆 Отметить выполненые достижения")
      },
      new List<KeyboardButton>() {
        new KeyboardButton("🥇 Рейтинг пользователей")
      }
    };
  }
}
