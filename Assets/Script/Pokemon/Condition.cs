using System;
using UnityEngine;
public class Condition 
{
    public string Name { get; set; }
    public int description { get; set; }
    public string StartMessage { get; set; }
    public Func<Pokemon, bool> OnBeforeMove; // Đã dùng ở trên
    public Action<Pokemon> OnStart { get; set; }           // ✅ Thêm dòng này
    public Action<Pokemon> OnAfterTurn { get; set; }
}
