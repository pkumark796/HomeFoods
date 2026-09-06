import { useState, useEffect, useCallback, type KeyboardEvent } from 'react';
import {
  TextField,
  InputAdornment,
  IconButton,
  Paper,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Box,
  Typography,
  CircularProgress,
} from '@mui/material';
import { Search as SearchIcon, Close as CloseIcon } from '@mui/icons-material';
import { productService } from '../services/api';
import { useNavigate } from 'react-router-dom';

interface SearchBarProps {
  onSearch?: (query: string) => void;
  fullWidth?: boolean;
}

export function SearchBar({ onSearch, fullWidth = false }: SearchBarProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const fetchSuggestions = useCallback(async (term: string) => {
    if (term.length < 2) {
      setSuggestions([]);
      return;
    }

    setLoading(true);
    try {
      const results = await productService.getSuggestions(term, 8);
      setSuggestions(results);
    } catch (error) {
      console.error('Error fetching suggestions:', error);
      setSuggestions([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      if (searchTerm) {
        fetchSuggestions(searchTerm);
      } else {
        setSuggestions([]);
      }
    }, 300);

    return () => clearTimeout(debounceTimer);
  }, [searchTerm, fetchSuggestions]);

  const handleSearch = () => {
    if (searchTerm.trim()) {
      setShowSuggestions(false);
      if (onSearch) {
        onSearch(searchTerm);
      } else {
        navigate(`/catalog?search=${encodeURIComponent(searchTerm)}`);
      }
    }
  };

  const handleSuggestionClick = (suggestion: string) => {
    setSearchTerm(suggestion);
    setShowSuggestions(false);
    if (onSearch) {
      onSearch(suggestion);
    } else {
      navigate(`/catalog?search=${encodeURIComponent(suggestion)}`);
    }
  };

  const handleClear = () => {
    setSearchTerm('');
    setSuggestions([]);
    setShowSuggestions(false);
  };

  const handleKeyPress = (e: KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <Box sx={{ position: 'relative', width: fullWidth ? '100%' : 'auto' }}>
      <TextField
        fullWidth={fullWidth}
        placeholder="Search for products..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        onFocus={() => setShowSuggestions(true)}
        onBlur={() => setTimeout(() => setShowSuggestions(false), 200)}
        onKeyDown={handleKeyPress}
        size="medium"
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon color="action" />
            </InputAdornment>
          ),
          endAdornment: (
            <InputAdornment position="end">
              {loading && <CircularProgress size={20} />}
              {searchTerm && !loading && (
                <IconButton size="small" onClick={handleClear}>
                  <CloseIcon fontSize="small" />
                </IconButton>
              )}
              {searchTerm && !loading && (
                <IconButton
                  color="primary"
                  onClick={handleSearch}
                  sx={{ ml: 1 }}
                >
                  <SearchIcon />
                </IconButton>
              )}
            </InputAdornment>
          ),
        }}
        sx={{
          bgcolor: 'background.paper',
          borderRadius: 2,
          '& .MuiOutlinedInput-root': {
            '&:hover fieldset': {
              borderColor: 'primary.main',
            },
          },
        }}
      />

      {/* Suggestions Dropdown */}
      {showSuggestions && suggestions.length > 0 && (
        <Paper
          elevation={3}
          sx={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            mt: 1,
            maxHeight: 400,
            overflow: 'auto',
            zIndex: 1000,
          }}
        >
          <List disablePadding>
            {suggestions.map((suggestion, index) => (
              <ListItem key={index} disablePadding>
                <ListItemButton onClick={() => handleSuggestionClick(suggestion)}>
                  <SearchIcon sx={{ mr: 2, color: 'text.secondary', fontSize: 20 }} />
                  <ListItemText
                    primary={suggestion}
                    primaryTypographyProps={{
                      variant: 'body2',
                    }}
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </Paper>
      )}

      {/* No Results */}
      {showSuggestions && !loading && searchTerm.length >= 2 && suggestions.length === 0 && (
        <Paper
          elevation={3}
          sx={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            mt: 1,
            p: 2,
            zIndex: 1000,
          }}
        >
          <Typography variant="body2" color="text.secondary" textAlign="center">
            No suggestions found
          </Typography>
        </Paper>
      )}
    </Box>
  );
}

export default SearchBar;
