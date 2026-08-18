import request from '../../core/request';
import type {
  DictionaryQueryInputDto,
  SaveDictionaryInputDto,
  SaveDictionaryItemInputDto,
} from './dtos/dictionary-input.dto';
import type {
  CreatedIdResultDto,
  DictionaryItemResultDto,
  DictionaryPageResultDto,
  DictionaryResultDto,
} from './dtos/dictionary-result.dto';

/** 字典分组与聚合内字典项的 API 服务。 */
export class DictionaryApiService {
  static getPageList(input: DictionaryQueryInputDto): Promise<DictionaryPageResultDto> {
    return request.post<DictionaryPageResultDto, DictionaryQueryInputDto>(
      '/api/dictionary/pagedQuery',
      input,
    );
  }

  static getById(id: string): Promise<DictionaryResultDto> {
    return request.get<DictionaryResultDto>(`/api/dictionary/${encodeURIComponent(id)}`);
  }

  static create(input: SaveDictionaryInputDto): Promise<CreatedIdResultDto> {
    return request.post<CreatedIdResultDto, SaveDictionaryInputDto>('/api/dictionary/create', input);
  }

  static update(id: string, input: SaveDictionaryInputDto): Promise<boolean> {
    return request.post<boolean, SaveDictionaryInputDto>(
      `/api/dictionary/update/${encodeURIComponent(id)}`,
      input,
    );
  }

  static getItems(dictionaryId: string): Promise<DictionaryItemResultDto[]> {
    return request.get<DictionaryItemResultDto[]>(
      `/api/dictionary/${encodeURIComponent(dictionaryId)}/items`,
    );
  }

  static createItem(
    dictionaryId: string,
    input: SaveDictionaryItemInputDto,
  ): Promise<CreatedIdResultDto> {
    return request.post<CreatedIdResultDto, SaveDictionaryItemInputDto>(
      `/api/dictionary/${encodeURIComponent(dictionaryId)}/item/create`,
      input,
    );
  }

  static updateItem(
    dictionaryId: string,
    itemId: string,
    input: SaveDictionaryItemInputDto,
  ): Promise<boolean> {
    return request.post<boolean, SaveDictionaryItemInputDto>(
      `/api/dictionary/${encodeURIComponent(dictionaryId)}/item/update/${encodeURIComponent(itemId)}`,
      input,
    );
  }
}
