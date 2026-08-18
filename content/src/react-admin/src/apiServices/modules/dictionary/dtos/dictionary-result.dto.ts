/** 字典分组列表行。 */
export interface DictionaryRowResultDto {
  id: string;
  systemId: string;
  systemName: string;
  name: string;
  itemCount: number;
}

/** 字典分组分页结果。 */
export interface DictionaryPageResultDto {
  totalCount: number;
  items: DictionaryRowResultDto[];
}

/** 字典项结果。 */
export interface DictionaryItemResultDto {
  id: string;
  dictionaryId: string;
  code: string;
  name: string;
  description: string | null;
  sort: number;
  isEnabled: boolean;
  isDefault: boolean;
  parentId: string | null;
}

/** 字典分组详情，包含聚合内全部字典项。 */
export interface DictionaryResultDto {
  id: string;
  systemId: string;
  systemName: string;
  name: string;
  items: DictionaryItemResultDto[];
}

/** 创建资源的响应。 */
export interface CreatedIdResultDto {
  id: string;
}
