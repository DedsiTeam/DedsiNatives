/** 系统列表单行结果。 */
export interface SystemRowResultDto {
  /** 系统唯一标识，26 位 ULID。 */
  id: string;
  /** 系统名称。 */
  name: string;
  /** 系统说明。 */
  description: string | null;
  /** 展示排序。 */
  sort: number;
}

/** 系统分页查询结果。 */
export interface SystemPageResultDto {
  /** 符合条件的记录总数。 */
  totalCount: number;
  /** 当前页系统数据。 */
  items: SystemRowResultDto[];
}

/** 系统详情结果。 */
export type SystemResultDto = SystemRowResultDto;
