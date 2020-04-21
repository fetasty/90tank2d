using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MsgID {
    GAME_START,         // 游戏开始 GameMode
    GAME_PAUSE,         // 游戏暂停 null
    GAME_RESUME,        // 游戏暂停恢复 null
    GAME_RETRY,         // 重新开始 null
    GAME_WIN,           // 游戏胜利 null
    GAME_OVER,          // 游戏结束 null
    ENEMY_SPAWN,        // 敌人spawner生成 Spawner
    ENEMY_BORN,         // 敌人生成 Enemy type
    ENEMY_DIE,          // 敌人死亡 Enemy type
    PLAYER_SPAWN,       // 玩家spawner生成 Spawner
    PLAYER_BORN,        // 玩家生成 Player id
    PLAYER_DIE,         // 玩家死亡 Pleyer id
    BONUS_SPAWN,        // 生成奖励
    BONUS_BOOM_TRIGGER, // 触发炸弹奖励
    BONUS_SHIELD_TRIGGER,   // 触发护盾奖励
    BONUS_TANK_TRIGGER,     // 触发加生命奖励
    BONUS_LEVEL_TRIGGER,    // 触发升级奖励
    BONUS_STOP_WATCH_TRIGGER,   // 触发秒表奖励
    BONUS_SHOVEL_TRIGGER,   // 触发铁锹奖励
    MOBILE_MOVE_INPUT,      // 移动端移动输入
    MOBILE_FIRE_INPUT,      // 移动端开火输入
}
