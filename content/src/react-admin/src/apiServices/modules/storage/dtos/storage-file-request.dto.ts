/**
 * @file 文件存储模块 - 请求参数 DTO 声明
 */

/**
 * 分页检索文件列表请求参数。
 */
export interface StorageFilePagedRequestDto {
  /** 文件名或存储名模糊检索关键字 */
  keyword?: string;
  /** 业务分类筛选（如 avatar, document, attachment） */
  category?: string;
  /** 文件扩展名筛选（如 .png, .pdf） */
  extension?: string;
  /** 存储介质筛选（1: Local, 2: MinIO, 3: AliyunOSS...） */
  storageType?: number;
  /** 是否公开筛选 */
  isPublic?: boolean;
  /** 上传起始时间（ISO 字符串） */
  startTimeUtc?: string;
  /** 上传截止时间（ISO 字符串） */
  endTimeUtc?: string;
  /** 当前页码，默认 1 */
  pageIndex?: number;
  /** 每页条数，默认 10 */
  pageSize?: number;
}
