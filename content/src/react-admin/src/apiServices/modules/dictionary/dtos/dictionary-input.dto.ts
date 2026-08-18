import type { PageInputDto } from '../../../core/base-dto';

/** 字典分组分页查询条件。 */
export interface DictionaryQueryInputDto extends PageInputDto {
  /** 所属系统标识。 */
  systemId?: string;
  /** 字典分组名称。 */
  name?: string;
}

/** 创建或更新字典分组的输入。 */
export interface SaveDictionaryInputDto {
  /** 所属系统标识。 */
  systemId: string;
  /** 字典分组名称。 */
  name: string;
}

/** 创建或更新字典项的输入。 */
export interface SaveDictionaryItemInputDto {
  /** 稳定业务编码。 */
  code: string;
  /** 显示名称。 */
  name: string;
  /** 字典项说明。 */
  description?: string | null;
  /** 展示排序。 */
  sort: number;
  /** 是否启用。 */
  isEnabled: boolean;
  /** 是否为默认项。 */
  isDefault: boolean;
  /** 父字典项标识。 */
  parentId?: string | null;
}
