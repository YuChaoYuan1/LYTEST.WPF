using System;
using System.Runtime.InteropServices;
namespace LYTest.WPF
{
    /// <summary>
    /// 系统休眠控制类
    /// </summary>
    /// <remarks>
    /// 该类提供了阻止系统进入休眠状态和恢复默认休眠策略的功能。
    /// 使用时请确保在不需要阻止休眠时调用 RestoreSleep 方法以避免影响系统性能。
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// SystemSleepManager.PreventSleep(); // 阻止系统休眠
    /// // ... 执行需要保持唤醒的操作 ...
    /// SystemSleepManager.RestoreSleep(); // 恢复系统默认休眠策略
    /// </example>
    public static class SystemSleepManager
    {
        /// <summary>
        /// <para>设置当前线程的执行状态，以防止系统进入休眠模式或关闭显示。</para>
        /// <para>Sets the execution state of the current thread to prevent the system from entering sleep mode or turning off
        /// the display.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// 使用该方法可以临时阻止系统进入休眠状态或关闭显示屏，适用于视频播放或长时间运行的任务等关键操作。
        /// </para>
        /// <para>Use this method to temporarily prevent the system from entering sleep mode or turning
        /// off the display during critical operations,  such as video playback or long-running tasks. Ensure that the
        /// execution state is reset to its previous value when the operation completes.</para></remarks>
        /// <param name="flags">A combination of <see cref="ExecutionFlag"/> values that specify the desired thread execution state.  For
        /// example, <see cref="ExecutionFlag.ES_CONTINUOUS"/> to make the state persistent, or <see
        /// cref="ExecutionFlag.ES_DISPLAY_REQUIRED"/>  to prevent the display from turning off.</param>
        /// <returns>The previous thread execution state as a combination of <see cref="ExecutionFlag"/> values.  Returns 0 if
        /// the operation fails.</returns>
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);
        /// <summary>
        /// <para>执行状态枚举</para>
        /// <para>Specifies execution states that can be used to prevent the system or display from entering a power-saving
        /// mode.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// 该枚举定义了可以传递给 <see cref="SetThreadExecutionState"/> 方法的标志位，用于控制系统的执行状态。
        /// </para>This enumeration is marked with the <see cref="FlagsAttribute"/>, allowing bitwise
        /// combination of its values. Use these flags with APIs that manage system execution states, such as preventing
        /// the system from sleeping or the display from turning off.</remarks>
        [Flags]
        enum ExecutionFlag : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,  // 阻止系统休眠
            ES_DISPLAY_REQUIRED = 0x00000002, // 阻止屏幕关闭
            ES_CONTINUOUS = 0x80000000       // 持续生效直至取消
        }
        /// <summary>
        /// <para>阻止系统进入休眠状态</para>
        /// Prevents the system from entering sleep mode or turning off the display.
        /// </summary>
        /// <remarks>This method ensures that the system remains active by setting thread execution state
        /// flags. It is useful for scenarios where continuous operation is required, such as during long-running
        /// processes or media playback.</remarks>
        public static void PreventSleep()
        {
            // 组合标志位，持续阻止休眠和屏幕关闭
            SetThreadExecutionState(ExecutionFlag.ES_SYSTEM_REQUIRED |
                                    ExecutionFlag.ES_DISPLAY_REQUIRED |
                                    ExecutionFlag.ES_CONTINUOUS);
        }
        /// <summary>
        /// <para>恢复系统默认休眠策略</para>
        /// Restores the system's default sleep policy.
        /// </summary>
        /// <remarks>This method resets the thread execution state to allow the system to enter sleep mode
        /// according to its default power management settings. It is typically used to undo changes made by methods
        /// that prevent the system from sleeping.</remarks>
        public static void RestoreSleep()
        {
            // 恢复系统默认休眠策略
            SetThreadExecutionState(ExecutionFlag.ES_CONTINUOUS);
        }
    }
}