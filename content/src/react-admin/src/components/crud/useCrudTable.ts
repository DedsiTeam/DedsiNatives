import { useCallback, useEffect, useRef, useState } from 'react';
import { message, type TablePaginationConfig } from 'antd';

/**
 * 分页数据响应接口契约
 */
export interface PagedResult<TItem> {
  items: TItem[];
  totalCount: number;
}

/**
 * useCrudTable 配置项参数
 */
export interface UseCrudTableOptions<TItem, TParams extends object = Record<string, never>> {
  /** 分页拉取数据接口，接收当前页码、条数与动态筛选条件 */
  fetchApi: (params: TParams & { pageIndex: number; pageSize: number }) => Promise<PagedResult<TItem>>;
  /** 删除接口（可选） */
  deleteApi?: (id: string) => Promise<unknown>;
  /** 默认单页记录条数，缺省为 10 */
  defaultPageSize?: number;
  /** 外部动态筛选条件对象（当值变化时自动触发查询刷新） */
  filters?: TParams;
  /** 筛选条件变化时是否自动回到第一页，默认为 true */
  resetPageOnFilterChange?: boolean;
  /** 页面挂载后是否自动触发初次加载，默认为 true */
  immediate?: boolean;
}

/**
 * 通用 CRUD 数据与分页状态管理 Hook
 */
export function useCrudTable<TItem, TParams extends object = Record<string, never>>(
  options: UseCrudTableOptions<TItem, TParams>
) {
  const {
    fetchApi,
    deleteApi,
    defaultPageSize = 10,
    filters = {} as TParams,
    resetPageOnFilterChange = true,
    immediate = true,
  } = options;

  const [items, setItems] = useState<TItem[]>([]);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [pageIndex, setPageIndex] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(defaultPageSize);
  const previousFiltersRef = useRef(filters);
  const latestRequestIdRef = useRef(0);

  /**
   * 核心加载数据方法
   */
  const loadData = useCallback(async () => {
    const requestId = ++latestRequestIdRef.current;
    setLoading(true);
    try {
      const queryParams = {
        ...filters,
        pageIndex,
        pageSize,
      } as TParams & { pageIndex: number; pageSize: number };

      const result = await fetchApi(queryParams);
      if (requestId !== latestRequestIdRef.current) return;
      setItems(result?.items || []);
      setTotalCount(result?.totalCount || 0);
    } catch {
      if (requestId !== latestRequestIdRef.current) return;
      setItems([]);
      setTotalCount(0);
    } finally {
      if (requestId === latestRequestIdRef.current) {
        setLoading(false);
      }
    }
  }, [fetchApi, filters, pageIndex, pageSize]);

  /**
   * 初始化及依赖条件变动时加载
   */
  useEffect(() => {
    const filtersChanged = previousFiltersRef.current !== filters;
    previousFiltersRef.current = filters;

    if (resetPageOnFilterChange && filtersChanged && pageIndex !== 1) {
      setPageIndex(1);
      return;
    }

    if (!immediate) {
      return () => {
        // 即使关闭自动加载，依赖变化或卸载时也要让尚未结束的手动请求失效。
        latestRequestIdRef.current += 1;
      };
    }
    const timeoutId = window.setTimeout(() => {
      void loadData();
    }, 0);

    return () => {
      window.clearTimeout(timeoutId);
      // 筛选或分页变化后立即废弃旧请求，避免旧响应覆盖即将发起的新查询。
      latestRequestIdRef.current += 1;
    };
  }, [filters, immediate, loadData, pageIndex, resetPageOnFilterChange]);

  /**
   * 手动刷新当前页（或重置到第一页）
   */
  const refresh = useCallback(
    (resetToFirstPage = false) => {
      if (resetToFirstPage) {
        setPageIndex(1);
      } else {
        void loadData();
      }
    },
    [loadData]
  );

  /**
   * 通用删除处理（自动处理删除当前页最后一条记录的自动回退边界）
   */
  const handleDelete = useCallback(
    async (id: string, successMessage = '删除成功') => {
      if (!deleteApi) {
        throw new Error('deleteApi is not configured in useCrudTable');
      }

      try {
        await deleteApi(id);
        message.success(successMessage);
        if (items.length === 1 && pageIndex > 1) {
          setPageIndex((prev) => prev - 1);
        } else {
          await loadData();
        }
        return true;
      } catch {
        return false;
      }
    },
    [deleteApi, items.length, loadData, pageIndex]
  );

  /**
   * 统一规范的分页配置对象（左右对齐、统一条数选项）
   */
  const pagination: TablePaginationConfig = {
    current: pageIndex,
    pageSize,
    total: totalCount,
    showTotal: (total, range) => `显示第 ${range[0]} - ${range[1]} 条，共 ${total} 条记录`,
    showSizeChanger: true,
    pageSizeOptions: ['10', '20', '50', '100', '500', '1000'],
    onChange: (nextPage, nextPageSize) => {
      setPageIndex(nextPageSize === pageSize ? nextPage : 1);
      setPageSize(nextPageSize);
    },
  };

  return {
    items,
    setItems,
    totalCount,
    loading,
    pageIndex,
    pageSize,
    setPageIndex,
    setPageSize,
    pagination,
    loadData,
    refresh,
    handleDelete,
  };
}
