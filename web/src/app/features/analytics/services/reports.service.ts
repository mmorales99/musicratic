import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import { Report, ReportDetail, ReportType } from "../models/report.model";

@Injectable({ providedIn: "root" })
export class ReportsService {
  private readonly api = inject(BffApiService);

  getReports(
    hubId: string,
    type: ReportType,
  ): Observable<Report[]> {
    const params = new URLSearchParams({ type });
    return this.api.get<Report[]>(
      `/analytics/hubs/${encodeURIComponent(hubId)}/reports?${params.toString()}`,
    );
  }

  getReportDetail(reportId: string): Observable<ReportDetail> {
    return this.api.get<ReportDetail>(
      `/analytics/reports/${encodeURIComponent(reportId)}`,
    );
  }
}
