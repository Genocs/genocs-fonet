<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:utilityExtension="pdfprinter:extensions:utility"
                xmlns:fo="http://www.w3.org/1999/XSL/Format">

	<xsl:variable name="logo" select="utilityExtension:MapPath('./assets/icon.jpg')"/>

	<xsl:template name="headerFirstPageTemplate">
		<fo:static-content flow-name="header-first-region">
			<fo:block font-size="6.5pt">
				<fo:table>
					<fo:table-column column-width="160mm"/>
					<fo:table-column column-width="30mm"/>
					<fo:table-body>
						<fo:table-row>
							<fo:table-cell>
								<fo:block font-size="9pt" text-align="left">
									<fo:block margin-bottom="0.1cm">
										<fo:inline font-weight="bold">Genocs</fo:inline> Software Technology and many more
									</fo:block>
									<fo:block margin-bottom="0.1cm">
										Tech company specialized in software development and IT consulting
									</fo:block>
									<fo:block margin-bottom="0.1cm">
										Via Trasimeno 40/10 - 20128 Milano (MI)
									</fo:block>
									<fo:block>										
										P.IVA: 03518950757
									</fo:block>
									<fo:block>
										<fo:basic-link external-destination="https://www.genocs.com" color="blue">https://www.genocs.com</fo:basic-link>
									</fo:block>
								</fo:block>

							</fo:table-cell>
							<fo:table-cell text-align="left">
								<fo:external-graphic src="url('{$logo}')" scaling="uniform" scaling-method="resample-any-method"/>
							</fo:table-cell>
						</fo:table-row>
					</fo:table-body>
				</fo:table>
			</fo:block>
		</fo:static-content>
	</xsl:template>

	<xsl:template name="headerOtherPagesTemplate">
		<fo:static-content flow-name="header-other-region">
			<fo:block font-size="8pt" space-after="2pt">
				<fo:table table-layout="fixed" width="100%">
					<fo:table-column column-width="proportional-column-width(1)"/>
					<fo:table-column column-width="18mm"/>
					<fo:table-body>
						<fo:table-row>
							<fo:table-cell display-align="center">
								<fo:block font-size="9pt" font-weight="bold">
									Genocs Software Technology
								</fo:block>
								<fo:block font-size="7pt" color="rgb(80, 80, 80)">
									Books Report
								</fo:block>
							</fo:table-cell>
							<fo:table-cell display-align="center" text-align="right">
								<fo:external-graphic src="url('{$logo}')" content-height="10mm" scaling="uniform" scaling-method="resample-any-method"/>
							</fo:table-cell>
						</fo:table-row>
					</fo:table-body>
				</fo:table>
			</fo:block>
			<fo:block>
				<fo:leader leader-pattern="rule" rule-thickness="0.5pt" leader-length="100%" color="rgb(128,128,128)"/>
			</fo:block>
		</fo:static-content>
	</xsl:template>
</xsl:stylesheet>
