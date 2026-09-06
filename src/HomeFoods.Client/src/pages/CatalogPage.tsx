import { useState, useEffect } from 'react';
import {
  Container,
  Box,
  Typography,
  Grid,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Drawer,
  IconButton,
} from '@mui/material';
import { FilterList as FilterListIcon, Close as CloseIcon } from '@mui/icons-material';
import { useSearchParams } from 'react-router-dom';
import { Product, Category, Brand } from '../types';
import { productService, categoryService, brandService, ProductFilterParams, PaginatedResponse } from '../services/api';
import ProductCard from '../components/ProductCard';
import FilterSidebar, { FilterState } from '../components/FilterSidebar';
import Breadcrumbs from '../components/Breadcrumbs';
import Pagination from '../components/Pagination';
import EmptyState from '../components/EmptyState';

type SortBy = ProductFilterParams['sortBy'];

const initialFilters: FilterState = {
  categoryId: undefined,
  brandIds: [],
  minPrice: 0,
  maxPrice: 1000,
  minDiscount: 0,
  inStock: false,
};

function CatalogPage() {
  const [searchParams] = useSearchParams();

  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState({
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
  });

  const [filters, setFilters] = useState<FilterState>({
    ...initialFilters,
    categoryId: searchParams.get('categoryId') || undefined,
    minDiscount: Number(searchParams.get('minDiscount')) || 0,
  });

  const [sortBy, setSortBy] = useState<SortBy>('name');
  const [sortDescending, setSortDescending] = useState(false);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  useEffect(() => {
    const fetchMetadata = async () => {
      try {
        const [categoriesData, brandsData] = await Promise.all([
          categoryService.getAll(),
          brandService.getAll(),
        ]);
        setCategories(categoriesData);
        setBrands(brandsData);
      } catch (error) {
        console.error('Error fetching metadata:', error);
      }
    };

    fetchMetadata();
  }, []);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);

        const filterParams: ProductFilterParams = {
          searchTerm: searchParams.get('search') || undefined,
          categoryId: filters.categoryId,
          brandIds: filters.brandIds.length > 0 ? filters.brandIds : undefined,
          minPrice: filters.minPrice > 0 ? filters.minPrice : undefined,
          maxPrice: filters.maxPrice < 1000 ? filters.maxPrice : undefined,
          minDiscount: filters.minDiscount > 0 ? filters.minDiscount : undefined,
          inStock: filters.inStock || undefined,
          sortBy,
          sortDescending,
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        const response: PaginatedResponse<Product> = await productService.getAll(filterParams);

        setProducts(response.items);
        setPagination({
          totalCount: response.totalCount,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalPages: response.totalPages,
        });
      } catch (error) {
        console.error('Error fetching products:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [filters, sortBy, sortDescending, pagination.pageNumber, pagination.pageSize, searchParams]);

  const handleFilterChange = (newFilters: FilterState) => {
    setFilters(newFilters);
    setPagination(prev => ({ ...prev, pageNumber: 1 }));
  };

  const handleClearFilters = () => {
    setFilters({
      categoryId: undefined,
      brandIds: [],
      minPrice: 0,
      maxPrice: 1000,
      minDiscount: 0,
      inStock: false,
    });
    setPagination(prev => ({ ...prev, pageNumber: 1 }));
  };

  const handleSortChange = (value: string) => {
    setSortBy(value as SortBy);
    setSortDescending(value === 'discount');
  };

  const handlePageChange = (page: number) => {
    setPagination(prev => ({ ...prev, pageNumber: page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handlePageSizeChange = (pageSize: number) => {
    setPagination(prev => ({ ...prev, pageSize, pageNumber: 1 }));
  };

  const selectedCategory = categories.find(c => c.id === filters.categoryId);
  const searchTerm = searchParams.get('search');

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100vh' }}>
      <Container maxWidth="xl" sx={{ py: 3 }}>
        {/* Breadcrumbs */}
        <Breadcrumbs
          items={[
            { label: 'Catalog', href: '/catalog' },
            ...(selectedCategory ? [{ label: selectedCategory.name }] : []),
          ]}
        />

        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box>
            <Typography variant="h4" fontWeight={700}>
              {searchTerm ? `Search Results for "${searchTerm}"` : selectedCategory?.name || 'All Products'}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {pagination.totalCount} products found
            </Typography>
          </Box>

          {/* Mobile Filter Button */}
          <IconButton
            sx={{ display: { xs: 'flex', md: 'none' } }}
            onClick={() => setMobileFiltersOpen(true)}
          >
            <FilterListIcon />
          </IconButton>
        </Box>

        {/* Sort and View Options */}
        <Box sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'center' }}>
          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Sort By</InputLabel>
            <Select
              value={sortBy}
              label="Sort By"
              onChange={(e) => handleSortChange(e.target.value)}
            >
              <MenuItem value="name">Name</MenuItem>
              <MenuItem value="price">Price: Low to High</MenuItem>
              <MenuItem value="discount">Discount: High to Low</MenuItem>
              <MenuItem value="newest">Newest First</MenuItem>
              <MenuItem value="popularity">Popularity</MenuItem>
            </Select>
          </FormControl>
        </Box>

        {/* Main Content */}
        <Grid container spacing={3}>
          {/* Filters Sidebar - Desktop */}
          <Grid item xs={12} md={3} sx={{ display: { xs: 'none', md: 'block' } }}>
            <FilterSidebar
              categories={categories}
              brands={brands}
              filters={filters}
              onFilterChange={handleFilterChange}
              onClearFilters={handleClearFilters}
            />
          </Grid>

          {/* Products Grid */}
          <Grid item xs={12} md={9}>
            {loading ? (
              <Grid container spacing={3}>
                {Array.from({ length: 8 }).map((_, index) => (
                  <Grid item xs={12} sm={6} lg={4} key={index}>
                    <LoadingCard />
                  </Grid>
                ))}
              </Grid>
            ) : products.length === 0 ? (
              <EmptyState
                title="No products found"
                message="Try adjusting your filters or search term to find what you're looking for."
                actionLabel="Clear Filters"
                onAction={handleClearFilters}
              />
            ) : (
              <>
                <Grid container spacing={3}>
                  {products.map((product) => (
                    <Grid item xs={12} sm={6} lg={4} key={product.id}>
                      <ProductCard product={product} />
                    </Grid>
                  ))}
                </Grid>

                {/* Pagination */}
                <Pagination
                  currentPage={pagination.pageNumber}
                  totalPages={pagination.totalPages}
                  pageSize={pagination.pageSize}
                  totalItems={pagination.totalCount}
                  onPageChange={handlePageChange}
                  onPageSizeChange={handlePageSizeChange}
                  pageSizeOptions={[12, 20, 40]}
                />
              </>
            )}
          </Grid>
        </Grid>
      </Container>

      {/* Mobile Filters Drawer */}
      <Drawer
        anchor="left"
        open={mobileFiltersOpen}
        onClose={() => setMobileFiltersOpen(false)}
        sx={{ display: { xs: 'block', md: 'none' } }}
      >
        <Box sx={{ width: 300, p: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" fontWeight={700}>
              Filters
            </Typography>
            <IconButton onClick={() => setMobileFiltersOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Box>
          <FilterSidebar
            categories={categories}
            brands={brands}
            filters={filters}
            onFilterChange={(newFilters) => {
              handleFilterChange(newFilters);
              setMobileFiltersOpen(false);
            }}
            onClearFilters={() => {
              handleClearFilters();
              setMobileFiltersOpen(false);
            }}
          />
        </Box>
      </Drawer>
    </Box>
  );
}

export default CatalogPage;
