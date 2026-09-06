import {
  Box,
  Typography,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Slider,
  Radio,
  RadioGroup,
  Divider,
  Button,
  Chip,
} from '@mui/material';
import { ExpandMore as ExpandMoreIcon, Close as CloseIcon } from '@mui/icons-material';
import { Category, Brand } from '../types';

export interface FilterState {
  categoryId?: string;
  brandIds: string[];
  minPrice: number;
  maxPrice: number;
  minDiscount: number;
  inStock: boolean;
}

interface FilterSidebarProps {
  categories: Category[];
  brands: Brand[];
  filters: FilterState;
  onFilterChange: (filters: FilterState) => void;
  onClearFilters: () => void;
}

export function FilterSidebar({
  categories,
  brands,
  filters,
  onFilterChange,
  onClearFilters,
}: FilterSidebarProps) {
  const handleCategoryChange = (categoryId: string) => {
    onFilterChange({
      ...filters,
      categoryId: filters.categoryId === categoryId ? undefined : categoryId,
    });
  };

  const handleBrandToggle = (brandId: string) => {
    const newBrandIds = filters.brandIds.includes(brandId)
      ? filters.brandIds.filter(id => id !== brandId)
      : [...filters.brandIds, brandId];

    onFilterChange({ ...filters, brandIds: newBrandIds });
  };

  const handlePriceChange = (_event: Event, newValue: number | number[]) => {
    const [min, max] = newValue as number[];
    onFilterChange({ ...filters, minPrice: min, maxPrice: max });
  };

  const handleDiscountChange = (minDiscount: number) => {
    onFilterChange({ ...filters, minDiscount });
  };

  const handleStockChange = (inStock: boolean) => {
    onFilterChange({ ...filters, inStock });
  };

  const hasActiveFilters = 
    filters.categoryId || 
    filters.brandIds.length > 0 || 
    filters.minPrice > 0 || 
    filters.maxPrice < 1000 ||
    filters.minDiscount > 0 ||
    filters.inStock;

  return (
    <Box sx={{ width: '100%', bgcolor: 'background.paper', borderRadius: 2, p: 2 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6" fontWeight={700}>
          Filters
        </Typography>
        {hasActiveFilters && (
          <Button
            size="small"
            onClick={onClearFilters}
            startIcon={<CloseIcon />}
            sx={{ textTransform: 'none' }}
          >
            Clear All
          </Button>
        )}
      </Box>

      <Divider sx={{ mb: 2 }} />

      {/* Categories */}
      <Accordion defaultExpanded disableGutters elevation={0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography fontWeight={600}>Category</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <RadioGroup
            value={filters.categoryId || ''}
            onChange={(e) => handleCategoryChange(e.target.value)}
          >
            {categories.map((category) => (
              <FormControlLabel
                key={category.id}
                value={category.id}
                control={<Radio size="small" />}
                label={category.name}
                sx={{ py: 0.5 }}
              />
            ))}
          </RadioGroup>
        </AccordionDetails>
      </Accordion>

      <Divider />

      {/* Brands */}
      <Accordion defaultExpanded disableGutters elevation={0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography fontWeight={600}>Brand</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <FormGroup>
            {brands.map((brand) => (
              <FormControlLabel
                key={brand.id}
                control={
                  <Checkbox
                    size="small"
                    checked={filters.brandIds.includes(brand.id)}
                    onChange={() => handleBrandToggle(brand.id)}
                  />
                }
                label={brand.name}
                sx={{ py: 0.5 }}
              />
            ))}
          </FormGroup>
        </AccordionDetails>
      </Accordion>

      <Divider />

      {/* Price Range */}
      <Accordion defaultExpanded disableGutters elevation={0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography fontWeight={600}>Price Range</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Box sx={{ px: 1 }}>
            <Slider
              value={[filters.minPrice, filters.maxPrice]}
              onChange={handlePriceChange}
              valueLabelDisplay="auto"
              min={0}
              max={1000}
              step={10}
              marks={[
                { value: 0, label: '$0' },
                { value: 1000, label: '$1000' },
              ]}
              sx={{ mt: 2 }}
            />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
              <Typography variant="body2" color="text.secondary">
                ${filters.minPrice}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                ${filters.maxPrice}
              </Typography>
            </Box>
          </Box>
        </AccordionDetails>
      </Accordion>

      <Divider />

      {/* Discount */}
      <Accordion defaultExpanded disableGutters elevation={0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography fontWeight={600}>Discount</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {[0, 10, 20, 30, 50].map((discount) => (
              <Chip
                key={discount}
                label={discount === 0 ? 'All Products' : `${discount}% or more`}
                onClick={() => handleDiscountChange(discount)}
                variant={filters.minDiscount === discount ? 'filled' : 'outlined'}
                color={filters.minDiscount === discount ? 'primary' : 'default'}
                sx={{ justifyContent: 'flex-start' }}
              />
            ))}
          </Box>
        </AccordionDetails>
      </Accordion>

      <Divider />

      {/* Availability */}
      <Accordion defaultExpanded disableGutters elevation={0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography fontWeight={600}>Availability</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <FormControlLabel
            control={
              <Checkbox
                checked={filters.inStock}
                onChange={(e) => handleStockChange(e.target.checked)}
              />
            }
            label="In Stock Only"
          />
        </AccordionDetails>
      </Accordion>
    </Box>
  );
}

export default FilterSidebar;
