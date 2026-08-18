import type { PageInputDto } from '../../../core/base-dto';

/** 系统分页查询参数。 */
export interface SystemQueryInputDto extends PageInputDto {
  /** 按系统名称模糊筛选。 */
  name?: string;
  /** 是否为导出查询。 */
  isExport?: boolean;
}

/** 创建系统请求参数。 */
export interface CreateSystemInputDto {
  /** 系统名称，不能为空。 */
  name: string;
  /** 系统说明，可为空。 */
  description?: string;
  /** 展示排序，数值越小越靠前。 */
  sort: number;
}

/** 更新系统请求参数。 */
export type UpdateSystemInputDto = CreateSystemInputDto;
