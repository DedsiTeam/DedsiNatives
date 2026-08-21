import request from '../../core/request';
import { DefaultApiServiceUrl } from '../../../configs';
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
   * 获取文件后端真实下载完整直链
   */
  static getDownloadUrl(id: string) {
    const base = DefaultApiServiceUrl || '';
    return `${base}/api/storage/download/${id}`;
  }

  /**
   * 获取文件后端真实预览/访问完整直链
   */
  static getPreviewUrl(id: string) {
    const base = DefaultApiServiceUrl || '';
    return `${base}/api/storage/preview/${id}`;
  }
}
