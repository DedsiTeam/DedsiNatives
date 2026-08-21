/**
 * @file 文件存储模块 - 响应结果 DTO 声明
 */

/**
 * 文件元数据结果模型。
 */
export interface StorageFileResultDto {
  /** 文件唯一标识，26 位 ULID */
  id: string;
  /** 用户原始文件名 */
  fileName: string;
  /** 物理存储文件名 */
  storageName: string;
  /** 文件扩展名 */
  extension: string;
  /** MIME 内容类型 */
  contentType: string;
  /** 文件大小（字节） */
  sizeBytes: number;
  /** 存储介质类型 (1: Local, 2: Minio, 3: AliyunOss...) */
  storageType: number;
  /** 相对存储路径 */
  relativePath: string;
  /** 访问直链或预览地址 */
  url?: string;
  /** MD5 摘要 */
  md5Hash?: string;
  /** 业务分类 */
  category: string;
  /** 是否公开可读 */
  isPublic: boolean;
  /** 文件说明 */
  description?: string;
  /** 上传时间 */
  createdAtUtc: string;
}

/**
 * 文件分页查询结果模型。
 */
export interface StorageFilePagedResultDto {
  /** 总记录数 */
  totalCount: number;
  /** 当前页文件记录 */
  items: StorageFileResultDto[];
}
