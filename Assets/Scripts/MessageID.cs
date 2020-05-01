using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageID
{
    GAME_START,         // 游戏开始
    GAME_PAUSE,         // 游戏暂停
    GAME_RESUME,        // 游戏暂停恢复
    GAME_RETRY,         // 重新开始
    GAME_INFO_UPDATE,   // 游戏数据刷新
    GAME_WIN,           // 游戏胜利
    GAME_OVER,          // 游戏结束
    ENEMY_SPAWN,        // 敌人spawner生成
    ENEMY_BORN,         // 敌人生成
    ENEMY_DIE,          // 敌人死亡
    PLAYER_SPAWN,       // 玩家spawner生成
    PLAYER_BORN,        // 玩家生成
    PLAYER_DIE,         // 玩家死亡
    BONUS_SPAWN,        // 生成奖励
    BONUS_BOOM_TRIGGER, // 触发炸弹奖励
    BONUS_SHIELD_TRIGGER,   // 触发护盾奖励
    BONUS_TANK_TRIGGER,     // 触发加生命奖励
    BONUS_LEVEL_TRIGGER,    // 触发升级奖励
    BONUS_STOP_WATCH_TRIGGER,   // 触发秒表奖励
    BONUS_SHOVEL_TRIGGER,   // 触发铁锹奖励
    MOBILE_MOVE_INPUT,      // 移动端移动输入
    MOBILE_FIRE_INPUT,      // 移动端开火输入
    HOME_DESTROY,           // 老家被毁
    DATA_GAME_START,            // 数据变动
    DATA_ENEMY_SPAWN,       // 数据变动-敌人生成
    DATA_ENEMY_DIE,         // 数据变动-敌人死亡
    DATA_PLAYER_SPAWN,      // 数据变动-玩家生成
    DATA_PLAYER_DIE,        // 数据变动-玩家死亡
    DATA_BONUS_TANK,        // 数据变动-触发tank奖励
}
