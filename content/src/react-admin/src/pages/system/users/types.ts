/**
 * @file 用户管理本地常量字典
 * @description 仅包含特殊 UI 级别的状态/渲染映射常量，不再包含任何重重复定义的数据类型（数据类型统一使用 ApiService DTO）
 */

/**
 * 角色对应标签名称与呈现颜色映射字典 (特殊 UI 映射)
 */
export const roleMap: Record<string, { label: string; color: string }> = {
  admin: { label: '超级管理员', color: 'geekblue' },
  developer: { label: '开发者', color: 'purple' },
  operator: { label: '运营人员', color: 'cyan' },
  user: { label: '普通用户', color: 'default' },
};
