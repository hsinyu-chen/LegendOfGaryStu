using Mortal.Core;
using Mortal.Story;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LegendOfGaryStu
{
    public static class RollStatAdditionHelper
    {
        public static Func<RollStatAddition, int> GetValue { get; private set; }

        static RollStatAdditionHelper()
        {
            var type = typeof(RollStatAddition);
            var x = Expression.Parameter(type);
            var valueMember = type.GetField("_currentValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            GetValue = Expression.Lambda<Func<RollStatAddition, int>>(Expression.Field(x, valueMember), x).Compile();
        }
    }

    public static class DiceMenuDialogHelper
    {
        public static Func<DiceMenuDialog, RollStatAddition[]> GetStats { get; private set; }
        public static Action<DiceMenuDialog, int> SetRandomValue { get; private set; }
        public static Func<DiceMenuDialog, bool> StartButtonPress { get; private set; }

        public static Func<DiceMenuDialog, bool> ReStartButtonPress { get; private set; }
        public static Func<DiceMenuDialog, MenuToggleButton> ReStartButton { get; private set; }
        static DiceMenuDialogHelper()
        {
            var type = typeof(DiceMenuDialog);
            var x = Expression.Parameter(type);
            var valueMember = type.GetProperty("RollStatAddition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            GetStats = Expression.Lambda<Func<DiceMenuDialog, RollStatAddition[]>>(Expression.Property(x, valueMember), x).Compile();

            var randomValueField = type.GetField("_currentRandomValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var intP = Expression.Parameter(typeof(int));
            SetRandomValue = Expression.Lambda<Action<DiceMenuDialog, int>>(Expression.Assign(Expression.Field(x, randomValueField), intP), x, intP).Compile();


            var _pressStartButtonMember = type.GetField("_pressStartButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var _pressRestartButtonMember = type.GetField("_pressRestartButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            StartButtonPress = Expression.Lambda<Func<DiceMenuDialog, bool>>(Expression.Field(x, _pressStartButtonMember), x).Compile();
            ReStartButtonPress = Expression.Lambda<Func<DiceMenuDialog, bool>>(Expression.Field(x, _pressRestartButtonMember), x).Compile();

            var _restartButtonMember = type.GetField("_restartButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ReStartButton = Expression.Lambda<Func<DiceMenuDialog, MenuToggleButton>>(Expression.Field(x, _restartButtonMember), x).Compile();
        }

        public static int GetStatsValue(DiceMenuDialog dialog)
        {
            var stats = GetStats(dialog);

            return stats.Sum(RollStatAdditionHelper.GetValue);
        }
    }
}
