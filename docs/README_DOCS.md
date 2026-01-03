# GitHub Pages Documentation

This directory contains the GitHub Pages site for the Blazor .NET Template documentation.

## Structure

- `index.md` - Landing page with links to all documentation
- `README.md` - Includes the main README.md from the root
- `CONTRIBUTING.md` - Includes the CONTRIBUTING.md from the root
- `IMPLEMENTATION_SUMMARY.md` - Includes the IMPLEMENTATION_SUMMARY.md from the root
- `_config.yml` - Jekyll configuration
- `_layouts/default.html` - Custom layout with navigation
- `Gemfile` - Ruby dependencies for local testing

## Local Testing

To test the site locally:

1. Install Ruby and Bundler
2. Install dependencies: `bundle install`
3. Run Jekyll: `bundle exec jekyll serve`
4. Open http://localhost:4000 in your browser

## Deployment

The site is automatically deployed via GitHub Actions when:
- Changes are pushed to the main branch in the docs directory
- Changes are made to README.md, CONTRIBUTING.md, or IMPLEMENTATION_SUMMARY.md
- Manually triggered via workflow_dispatch

## Enabling GitHub Pages

To enable GitHub Pages for this repository:

1. Go to repository Settings > Pages
2. Under "Source", select "GitHub Actions"
3. The site will be available at: https://mlmdevs.github.io/blazor-net-template/

## Theme

This site uses the Cayman theme with custom navigation added via `_layouts/default.html`.
