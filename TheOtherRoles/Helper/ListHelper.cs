using System;
using Il2CppSystem.Collections.Generic;

namespace TheOtherRoles.Helper;

public static class ListHelper
{
    public static T Get<T>(this List<T> list, int index) => list._items[index];
    
    public static T Get<T>(this List<T> list, Index index) => list._items[index];
}