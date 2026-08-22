import request from '../../core/request';
import type { StorageFilePagedRequestDto } from './dtos/storage-file-request.dto';
import type {
  StorageFilePagedResultDto,
  StorageFileResultDto,
} from './dtos/storage-file-result.dto';

/**
 * 文件与对象存储 API 服务
 */
export class StorageApiService {
  /**
   * 分页查询文件记录列表
   */
  static getStorageFilesPaged(params: StorageFilePagedRequestDto) {
    return request.post<StorageFilePagedResultDto>('/api/storage/pagedQuery', params);
  }

  /**
   * 获取文件详情
   */
  static getStorageFileDetail(id: string) {
    return request.get<StorageFileResultDto>(`/api/storage/${id}`);
  }

  /**
   * 上传文件
   * @param file 待上传的文件对象
   * @param category 业务分类
   * @param isPublic 是否公开
   * @param description 描述
   */
  static uploadFile(
    file: File,
    category = 'general',
    isPublic = false,
    description?: string
  ) {
    const formData = new FormData();
    formData.append('File', file);
    formData.append('Category', category);
    formData.append('IsPublic', String(isPublic));
    if (description) {
      formData.append('Description', description);
    }

    return request.post<StorageFileResultDto>('/api/storage/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  /**
   * 删除文件
   */
  static deleteStorageFile(id: string) {
    return request.post<{ success: boolean }>(`/api/storage/delete/${id}`);
  }

  /**
   * 使用当前登录令牌下载文件内容。
   */
  static downloadFile(id: string): Promise<Blob> {
    return request.get<Blob>(`/api/storage/download/${encodeURIComponent(id)}`, {
      responseType: 'blob',
    });
  }

  /**
   * 使用当前登录令牌读取文件预览内容。
   */
  static previewFile(id: string): Promise<Blob> {
    return request.get<Blob>(`/api/storage/preview/${encodeURIComponent(id)}`, {
      responseType: 'blob',
    });
  }
}
