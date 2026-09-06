#!/usr/bin/env sh

# https://github.com/jakobhellermann/rabex-cli
rabex --steam-game 'Hollow Knight: Silksong' script TransitionPoint references --format json \
  | jq -c 'reduce (.referrers[] | { scene, label: (.label | sub("@TransitionPoint"; "") | split("/") | last) }) as $point
      ({}; .[$point.scene] += [$point.label])
      | with_entries(.value |= (sort | unique))' \
  > CommandPalette/TransitionPoints.json
