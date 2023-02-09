using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseProjectBot.Models {
  public class Game {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Achievements_List { get; set; }

    public List<int> GetAchievements_List() {
      var str_List = this.Achievements_List.Split(';').ToList();

      List<int> ach_List = new List<int>();
      foreach (var item in str_List)
        ach_List.Add(Int32.Parse(item));

      return ach_List;
    }
    public void SetAchievements_List(List<int> lst) {
      Achievements_List = "";
      foreach (var item in lst)
        Achievements_List += item.ToString() + ';';

      Achievements_List.Remove(Achievements_List.Length - 1);
    }
  }
}
