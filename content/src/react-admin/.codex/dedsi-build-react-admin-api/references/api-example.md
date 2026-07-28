# API 示例代码

以下示例以 `Product` 模块展示标准 DTO、Service 和统一导出方式。生成代码时复用结构和写法，不照抄业务字段。

## 目录

```text
src/apiServices/modules/product/
├─ dtos/
│  ├─ product-input.dto.ts
│  └─ product-result.dto.ts
└─ product.service.ts
```

## 输入 DTO

文件：`dtos/product-input.dto.ts`

```ts
import type { PageInputDto } from '../../../core/base-dto';

/**
 * 产品分页查询参数。
 */
export interface ProductQueryInputDto extends PageInputDto {
  /** 产品名称筛选条件，为空时不按名称过滤。 */
  name?: string;
  /** 是否按启用状态筛选。 */
  isEnabled?: boolean;
}

/**
 * 创建产品的请求参数。
 */
export interface CreateProductInputDto {
  /** 产品名称，不能为空。 */
  name: string;
  /** 产品单价，单位为元且不能小于零。 */
  price: number;
}

/**
 * 更新产品的请求参数。
 */
export interface UpdateProductInputDto {
  /** 产品名称，不能为空。 */
  name: string;
  /** 产品单价，单位为元且不能小于零。 */
  price: number;
}
```

## 结果 DTO

文件：`dtos/product-result.dto.ts`

```ts
/**
 * 产品列表中的单行数据。
 */
export interface ProductRowResultDto {
  /** 产品唯一标识。 */
  id: string;
  /** 产品名称。 */
  name: string;
  /** 产品单价，单位为元。 */
  price: number;
  /** 产品是否启用。 */
  isEnabled: boolean;
}

/**
 * 产品分页查询结果。
 */
export interface ProductPageResultDto {
  /** 符合条件的记录总数。 */
  totalCount: number;
  /** 当前页的产品数据。 */
  items: ProductRowResultDto[];
}

/**
 * 产品详情结果。
 */
export interface ProductResultDto extends ProductRowResultDto {
  /** 产品创建时间，使用后端返回的 ISO 8601 字符串。 */
  creationTime: string;
}
```

## API Service

文件：`product.service.ts`

```ts
import request from '../../core/request';
import type {
  CreateProductInputDto,
  ProductQueryInputDto,
  UpdateProductInputDto,
} from './dtos/product-input.dto';
import type {
  ProductPageResultDto,
  ProductResultDto,
} from './dtos/product-result.dto';

/**
 * 产品模块 API 服务。
 */
export class ProductApiService {
  /**
   * 分页查询产品。
   * @param input 分页和筛选条件。
   */
  static getPageList(input: ProductQueryInputDto): Promise<ProductPageResultDto> {
    return request.post<ProductPageResultDto>('/api/product/pagedQuery', input);
  }

  /**
   * 获取产品详情。
   * @param id 产品唯一标识。
   */
  static getById(id: string): Promise<ProductResultDto> {
    return request.get<ProductResultDto>(
      `/api/product/${encodeURIComponent(id)}`,
    );
  }

  /**
   * 创建产品并返回新产品标识。
   * @param input 创建产品参数。
   */
  static create(input: CreateProductInputDto): Promise<string> {
    return request.post<string>('/api/product/create', input);
  }

  /**
   * 更新指定产品。
   * @param id 产品唯一标识。
   * @param input 更新产品参数。
   */
  static update(id: string, input: UpdateProductInputDto): Promise<boolean> {
    return request.post<boolean>(
      `/api/product/update/${encodeURIComponent(id)}`,
      input,
    );
  }

  /**
   * 删除指定产品。
   * @param id 产品唯一标识。
   */
  static delete(id: string): Promise<boolean> {
    return request.post<boolean>(
      `/api/product/delete/${encodeURIComponent(id)}`,
    );
  }
}
```

## 统一导出

在 `src/apiServices/index.ts` 添加：

```ts
export { ProductApiService } from './modules/product/product.service';
export type {
  CreateProductInputDto,
  ProductQueryInputDto,
  UpdateProductInputDto,
} from './modules/product/dtos/product-input.dto';
export type {
  ProductPageResultDto,
  ProductResultDto,
  ProductRowResultDto,
} from './modules/product/dtos/product-result.dto';
```

## 请求客户端的无 any 写法

修改通用客户端时采用准确泛型：

```ts
public get<TResponse>(
  url: string,
  config?: AxiosRequestConfig,
): Promise<TResponse> {
  return this.instance.get<unknown, TResponse>(url, config);
}

public post<TResponse, TBody = unknown>(
  url: string,
  data?: TBody,
  config?: AxiosRequestConfig<TBody>,
): Promise<TResponse> {
  return this.instance.post<unknown, TResponse, TBody>(url, data, config);
}
```

必须结合当前 Axios 版本验证泛型签名。若响应拦截器已经返回 `response.data`，Service 直接接收 `TResponse`，不能再次访问 `.data`。
