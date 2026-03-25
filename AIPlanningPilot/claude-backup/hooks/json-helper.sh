#!/usr/bin/env bash
# json-helper.sh — Robust JSON field extraction using Node.js
#
# Sourced by hook scripts. Provides json_get() for reliable
# parsing of Claude Code tool input JSON.
#
# Why node? jq is not available on this system's Git Bash.
# Node.js is guaranteed (Nx/Angular project).
#
# Usage:
#   source "$(dirname "$0")/json-helper.sh"
#   VALUE=$(echo "$JSON" | json_get "tool_input.file_path")

json_get() {
  local field_path="$1"
  node -e "
    let d='';
    process.stdin.setEncoding('utf8');
    process.stdin.on('data',c=>d+=c);
    process.stdin.on('end',()=>{
      try{
        let o=JSON.parse(d);
        for(const p of '${field_path}'.split('.'))o=o?.[p];
        if(o!=null)process.stdout.write(String(o));
      }catch(e){}
    });
  "
}
